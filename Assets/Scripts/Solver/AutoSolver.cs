using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Pieces;
using Rules;
using Rules.AspectSources;
using Rules.CompletionRules;
using Rules.EmotionRules;
using Scenarios;
using UnityEngine;
using Zones;

namespace Solver
{
    /// <summary>
    /// Exhaustive backtracking solver. Two structural speedups over a naive approach:
    ///
    /// 1. Canonical anchor ordering — each piece placed must have its lex-min tile (anchor)
    ///    strictly greater (row-major) than the previous piece's anchor. Non-overlapping pieces
    ///    always have distinct anchors, so every valid board configuration is visited exactly
    ///    once instead of N! times.
    ///
    /// 2. Parallel top-level dispatch — every valid first-piece placement is an independent
    ///    subtree handed to Parallel.ForEach across all CPU cores.
    ///
    /// Each worker owns pre-allocated flat arrays and reusable buffers to minimise GC pressure.
    /// Rules/zones/aspect-sources are stateless and shared safely across workers.
    /// </summary>
    public class AutoSolver
    {
        // Progress counters written by background threads, read by main thread.
        // int fields are volatile (32-bit reads are atomic). long fields require
        // Interlocked.Read for safe cross-thread access.
        public long TriedCount;
        public volatile int FoundCount;
        public volatile int BestScore;
        public long TopLevelTriedCount;
        public long TotalTopLevelPositions;

        private readonly int _width;
        private readonly int _height;
        private readonly List<Piece> _availablePieces;
        private readonly int _maxResults;
        private readonly int _totalPieceSlots; // locked + available

        private readonly List<EmotionRule> _emotionRules;
        private readonly List<CompletionRuleConfig> _completionRuleConfigs;
        private readonly List<Zone> _clonedZones;
        private readonly List<AspectSource> _aspectSources;
        private readonly List<Vector2Int> _blockedList; // shared read-only ref for GameState

        // Per-piece precomputed placements (in-bounds, non-blocked, sorted by anchor).
        // Stored as arrays for ref-readonly element access with zero struct copy.
        private readonly PrecomputedPlacement[][] _precomputed;

        // For each piece index i, indices j < i where _availablePieces[j].sourceSO == _availablePieces[i].sourceSO.
        // Used to skip non-canonical orderings of identical pieces and eliminate K! redundancy.
        private readonly int[][] _lowerIdenticalNeighbors;

        // Pruning flags set at construction time
        private readonly bool _forwardCheckEnabled;   // Flag 1: forward-check each remaining piece is still placeable
        private readonly bool _fullCoverageEnabled;   // Flag 3: require all empty cells filled; prune via component check

        // Precomputed for full-coverage component check (Flag 3)
        private readonly bool[] _blockedFlat;              // flat [y*w+x], true if cell is blocked
        private readonly int _totalAvailableTiles;          // sum of tile counts of all available pieces
        private readonly int[] _availablePieceTileCounts;  // per-piece tile counts

        // Wall sets — used to exclude placements that span a wall, mirroring BoardController.CrossesWall
        private readonly HashSet<Vector2Int> _horizontalWallSet;
        private readonly HashSet<Vector2Int> _verticalWallSet;

        // Initial board state from locked pieces — copied into each worker.
        private readonly List<PlacedPiece> _lockedPlacements;
        private readonly bool[] _initialOccupied;      // flat [y * _width + x]
        private readonly PlacedPiece[,] _initialTileArray; // [x, y]

        private readonly List<SolverResult> _results = new();
        private readonly object _resultsLock = new();

        // Sentinel: lex-less than any valid board position (all coords ≥ 0)
        private static readonly Vector2Int InitialMinAnchor = new(int.MinValue, int.MinValue);

        // -------------------------------------------------------------------------
        // Precomputed placement entry
        // -------------------------------------------------------------------------
        private readonly struct PrecomputedPlacement
        {
            public readonly Vector2Int Anchor;    // lex-min tile — used for canonical ordering
            public readonly Vector2Int Position;  // piece origin passed to PlacedPiece ctor
            public readonly int Rotation;         // 0-3
            public readonly Vector2Int[] Tiles;   // board cells for TileArray updates
            public readonly int[] TileIndices;    // flat [y*w+x] indices for occupancy check

            public PrecomputedPlacement(
                Vector2Int anchor, Vector2Int position, int rotation,
                Vector2Int[] tiles, int[] tileIndices)
            {
                Anchor = anchor; Position = position; Rotation = rotation;
                Tiles = tiles; TileIndices = tileIndices;
            }
        }

        // -------------------------------------------------------------------------
        // Per-worker context — allocated once per top-level subtree
        // -------------------------------------------------------------------------
        private sealed class WorkerContext
        {
            public readonly bool[] Occupied;           // flat occupancy board
            public readonly PlacedPiece[,] TileArray;  // incremental 2-D board for EmotionContext
            public readonly bool[] PieceUsed;
            public readonly PlacedPiece[] Placements;  // locked pieces first, then available
            public int PlacementCount;
            public readonly List<PlacedPiece> PlacementsList;       // reused as GameState.PlacedPieces
            public readonly List<PieceEmotionState> PieceStatesBuffer; // reused for EmotionEvaluationResult
            public readonly List<EmotionEffect> EffectsBuffer;      // reused per-piece effect accumulation
            public readonly GameState GameState;                     // reused across all terminals

            // Reusable buffers for pruning checks (allocated once per worker)
            public readonly bool[] Visited;          // BFS visited flags (component detection)
            public readonly int[] BfsQueue;          // BFS queue
            public readonly bool[] AchievableBuffer; // 0/1 knapsack DP for component-size check
            public int RemainingPieceTiles;          // total tile-count of unplaced available pieces

            public WorkerContext(
                int width, int height, int availCount, int totalSlots,
                bool[] initialOccupied, PlacedPiece[,] initialTileArray,
                List<PlacedPiece> lockedPlacements,
                List<EmotionRule> emotionRules, List<CompletionRuleConfig> completionRules,
                List<Zone> zones, List<AspectSource> aspectSources, List<Vector2Int> blockedList,
                int totalAvailableTiles)
            {
                Occupied = (bool[])initialOccupied.Clone();
                TileArray = (PlacedPiece[,])initialTileArray.Clone();
                PieceUsed = new bool[availCount];
                Placements = new PlacedPiece[totalSlots];
                PlacementCount = lockedPlacements.Count;
                for (int i = 0; i < lockedPlacements.Count; i++)
                    Placements[i] = lockedPlacements[i];
                PlacementsList = new List<PlacedPiece>(totalSlots);
                PieceStatesBuffer = new List<PieceEmotionState>(totalSlots);
                EffectsBuffer = new List<EmotionEffect>(emotionRules.Count + 1);
                GameState = new GameState(
                    new Vector2Int(width, height),
                    blockedList,   // shared read-only reference — completion rules don't mutate it
                    null,          // PlacedPieces is set per terminal call
                    new List<Piece>(),
                    null,
                    emotionRules,
                    completionRules,
                    zones,
                    aspectSources);
                Visited = new bool[width * height];
                BfsQueue = new int[width * height];
                AchievableBuffer = new bool[totalAvailableTiles + 1];
                RemainingPieceTiles = totalAvailableTiles;
            }
        }

        // -------------------------------------------------------------------------
        // Construction (must run on the main thread)
        // -------------------------------------------------------------------------
        public AutoSolver(ScenarioSO scenario, int maxResults,
            bool forwardCheck = false, bool fullCoverage = false)
        {
            _maxResults = maxResults;
            _width = scenario.gridSize.x;
            _height = scenario.gridSize.y;
            _availablePieces = scenario.AvailablePieces();
            _emotionRules = new List<EmotionRule>(scenario.emotionRules);
            _completionRuleConfigs = new List<CompletionRuleConfig>(scenario.completionRules);
            _clonedZones = scenario.Zones();
            _aspectSources = scenario.AspectSources();
            _blockedList = new List<Vector2Int>(scenario.blockedPositions);

            _forwardCheckEnabled = forwardCheck;
            _fullCoverageEnabled = fullCoverage;

            _horizontalWallSet = new HashSet<Vector2Int>(scenario.horizontalWalls ?? new List<Vector2Int>());
            _verticalWallSet   = new HashSet<Vector2Int>(scenario.verticalWalls   ?? new List<Vector2Int>());

            _blockedFlat = new bool[_width * _height];
            foreach (var pos in scenario.blockedPositions)
                _blockedFlat[pos.y * _width + pos.x] = true;

            _availablePieceTileCounts = new int[_availablePieces.Count];
            _totalAvailableTiles = 0;
            for (int i = 0; i < _availablePieces.Count; i++)
            {
                _availablePieceTileCounts[i] = _availablePieces[i].shape.Count;
                _totalAvailableTiles += _availablePieceTileCounts[i];
            }

            var blockedSet = new HashSet<Vector2Int>(scenario.blockedPositions);

            _lockedPlacements = scenario.LockedPieces();
            _totalPieceSlots = _lockedPlacements.Count + _availablePieces.Count;

            // Build flat initial board from locked pieces
            _initialOccupied = new bool[_width * _height];
            _initialTileArray = new PlacedPiece[_width, _height];
            foreach (var locked in _lockedPlacements)
            foreach (var tile in locked.GetTilePosition())
            {
                _initialOccupied[tile.y * _width + tile.x] = true;
                _initialTileArray[tile.x, tile.y] = locked;
            }

            // Precompute valid placements per piece.
            // Excludes out-of-bounds, blocked cells, and cells occupied by locked pieces.
            // Occupancy from available pieces is dynamic — checked at search time.
            _precomputed = new PrecomputedPlacement[_availablePieces.Count][];
            for (int i = 0; i < _availablePieces.Count; i++)
            {
                var piece = _availablePieces[i];
                var uniqueRotations = ComputeUniqueRotations(piece.shape);
                var placements = new List<PrecomputedPlacement>();

                foreach (int rotation in uniqueRotations)
                for (int y = 0; y < _height; y++)
                for (int x = 0; x < _width; x++)
                {
                    var tiles = ComputeTilesArray(piece.shape, rotation, new Vector2Int(x, y));
                    if (!IsStaticallyValid(tiles, blockedSet)) continue;

                    var indices = new int[tiles.Length];
                    for (int t = 0; t < tiles.Length; t++)
                        indices[t] = tiles[t].y * _width + tiles[t].x;

                    var anchor = ComputeAnchor(tiles);
                    placements.Add(new PrecomputedPlacement(anchor, new Vector2Int(x, y), rotation, tiles, indices));
                }

                placements.Sort((a, b) => LexCompare(a.Anchor, b.Anchor));
                _precomputed[i] = placements.ToArray();
            }

            // Precompute identical-piece groups to eliminate K! redundancy.
            // piece[i] may only be placed when all lower-indexed identical pieces are already placed.
            _lowerIdenticalNeighbors = new int[_availablePieces.Count][];
            for (int i = 0; i < _availablePieces.Count; i++)
            {
                var lower = new List<int>();
                for (int j = 0; j < i; j++)
                    if (_availablePieces[j].sourceSO == _availablePieces[i].sourceSO)
                        lower.Add(j);
                _lowerIdenticalNeighbors[i] = lower.ToArray();
            }
        }

        // -------------------------------------------------------------------------
        // Entry point
        // -------------------------------------------------------------------------
        public Task<List<SolverResult>> SolveAsync(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                if (_availablePieces.Count == 0)
                {
                    var ctx0 = CreateWorkerContext();
                    if (!_fullCoverageEnabled || PassesFullCoverageCheck(ctx0))
                        EvaluateTerminal(ctx0);
                    return GetResults();
                }

                // Every valid first-piece placement is an independent subtree.
                var topLevel = new List<(int PieceIdx, int PlacIdx)>();
                for (int i = 0; i < _availablePieces.Count; i++)
                {
                    if (_lowerIdenticalNeighbors[i].Length > 0) continue; // skip non-first identical pieces
                    for (int j = 0; j < _precomputed[i].Length; j++)
                        topLevel.Add((i, j));
                }

                TotalTopLevelPositions = topLevel.Count;

                try
                {
                    Parallel.ForEach(
                        topLevel,
                        new ParallelOptions
                        {
                            MaxDegreeOfParallelism = Environment.ProcessorCount,
                            CancellationToken = ct
                        },
                        item =>
                        {
                            Interlocked.Increment(ref TopLevelTriedCount);

                            var (pieceIdx, placIdx) = item;
                            ref readonly var precomp = ref _precomputed[pieceIdx][placIdx];
                            var ctx = CreateWorkerContext();

                            // Place first piece into worker's local state
                            ctx.PieceUsed[pieceIdx] = true;
                            var placed = new PlacedPiece(
                                _availablePieces[pieceIdx], precomp.Rotation, precomp.Position);
                            ctx.Placements[ctx.PlacementCount++] = placed;
                            ctx.RemainingPieceTiles -= precomp.TileIndices.Length;
                            foreach (var idx in precomp.TileIndices)
                                ctx.Occupied[idx] = true;
                            foreach (var tile in precomp.Tiles)
                                ctx.TileArray[tile.x, tile.y] = placed;

                            bool canRecurse = true;
                            if (_forwardCheckEnabled)
                                canRecurse = PassesForwardCheck(ctx, precomp.Anchor);
                            if (canRecurse && _fullCoverageEnabled)
                                canRecurse = PassesFullCoverageCheck(ctx);
                            if (canRecurse)
                                Backtrack(precomp.Anchor, ctx, ct);
                        });
                }
                catch (OperationCanceledException) { }

                return GetResults();
            }, ct);
        }

        public List<SolverResult> GetResults()
        {
            lock (_resultsLock)
                return new List<SolverResult>(_results);
        }

        // -------------------------------------------------------------------------
        // Backtracking search
        // -------------------------------------------------------------------------

        /// <summary>
        /// Canonical-ordering constraint: the next piece's anchor must be lex-greater than
        /// <paramref name="minAnchor"/>. Since non-overlapping pieces always have distinct
        /// anchors, this guarantees each board configuration is visited exactly once.
        /// </summary>
        private void Backtrack(Vector2Int minAnchor, WorkerContext ctx, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;

            if (ctx.PlacementCount == _totalPieceSlots)
            {
                Interlocked.Increment(ref TriedCount);
                EvaluateTerminal(ctx);
                return;
            }

            for (int pieceIdx = 0; pieceIdx < _availablePieces.Count; pieceIdx++)
            {
                if (ctx.PieceUsed[pieceIdx]) continue;
                if (HasUnplacedLowerIdentical(pieceIdx, ctx.PieceUsed)) continue;

                var arr = _precomputed[pieceIdx];
                // Binary search: jump past entries with anchor ≤ minAnchor
                int start = FindFirstGreater(arr, minAnchor);

                for (int j = start; j < arr.Length; j++)
                {
                    ref readonly var precomp = ref arr[j];

                    if (OccupancyConflict(precomp.TileIndices, ctx.Occupied)) continue;

                    // Place
                    ctx.PieceUsed[pieceIdx] = true;
                    var placed = new PlacedPiece(
                        _availablePieces[pieceIdx], precomp.Rotation, precomp.Position);
                    ctx.Placements[ctx.PlacementCount++] = placed;
                    ctx.RemainingPieceTiles -= precomp.TileIndices.Length;
                    foreach (var idx in precomp.TileIndices)
                        ctx.Occupied[idx] = true;
                    foreach (var tile in precomp.Tiles)
                        ctx.TileArray[tile.x, tile.y] = placed;

                    bool canRecurse = true;
                    if (_forwardCheckEnabled)
                        canRecurse = PassesForwardCheck(ctx, precomp.Anchor);
                    if (canRecurse && _fullCoverageEnabled)
                        canRecurse = PassesFullCoverageCheck(ctx);
                    if (canRecurse)
                        Backtrack(precomp.Anchor, ctx, ct);

                    // Unplace
                    ctx.PieceUsed[pieceIdx] = false;
                    ctx.PlacementCount--;
                    ctx.RemainingPieceTiles += precomp.TileIndices.Length;
                    foreach (var idx in precomp.TileIndices)
                        ctx.Occupied[idx] = false;
                    foreach (var tile in precomp.Tiles)
                        ctx.TileArray[tile.x, tile.y] = null;
                }
            }
        }

        // -------------------------------------------------------------------------
        // Terminal evaluation
        // -------------------------------------------------------------------------
        private void EvaluateTerminal(WorkerContext ctx)
        {
            // Refresh GameState.PlacedPieces without allocating a new list
            ctx.PlacementsList.Clear();
            for (int i = 0; i < ctx.PlacementCount; i++)
                ctx.PlacementsList.Add(ctx.Placements[i]);
            ctx.GameState.PlacedPieces = ctx.PlacementsList;

            // ctx.TileArray is kept in sync during search — no ConvertTiles needed
            var context = new EmotionContext(ctx.GameState, ctx.TileArray, _clonedZones);

            // Aspect-source phase: mirrors RulesController.Evaluate
            for (int i = 0; i < ctx.PlacementCount; i++)
                ctx.Placements[i].DynamicAspects.Clear();
            for (int i = 0; i < ctx.PlacementCount; i++)
            for (int s = 0; s < _aspectSources.Count; s++)
                _aspectSources[s]?.Apply(ctx.Placements[i], context);

            // Build emotion states without LINQ
            ctx.PieceStatesBuffer.Clear();
            for (int i = 0; i < ctx.PlacementCount; i++)
            {
                var placed = ctx.Placements[i];
                if (!placed.Piece.hasEmotions) continue;

                ctx.EffectsBuffer.Clear();
                for (int r = 0; r < _emotionRules.Count; r++)
                {
                    var rule = _emotionRules[r];
                    if (rule == null) continue;
                    var effect = rule.Evaluate(placed, context);
                    if (effect != null) ctx.EffectsBuffer.Add(effect);
                }
                // PieceEmotionState stores Effects for UI — must be a fresh list
                ctx.PieceStatesBuffer.Add(
                    new PieceEmotionState(placed, new List<EmotionEffect>(ctx.EffectsBuffer)));
            }

            var emotionResult = new EmotionEvaluationResult(ctx.PieceStatesBuffer);

            // Check completion rules (loop avoids LINQ .All allocation)
            for (int c = 0; c < _completionRuleConfigs.Count; c++)
                if (!_completionRuleConfigs[c].rule.IsMet(emotionResult, ctx.GameState)) return;

            int score = emotionResult.Score;
            var snapshot = new List<PlacedPiece>(ctx.PlacementCount);
            for (int i = 0; i < ctx.PlacementCount; i++)
                snapshot.Add(ctx.Placements[i]);
            var result = new SolverResult(snapshot, score, true);

            lock (_resultsLock)
            {
                int insertAt = _results.Count;
                for (int i = 0; i < _results.Count; i++)
                    if (score > _results[i].Score) { insertAt = i; break; }
                _results.Insert(insertAt, result);
                if (_results.Count > _maxResults) _results.RemoveAt(_results.Count - 1);
                BestScore = _results[0].Score;
            }
            Interlocked.Increment(ref FoundCount);
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------
        private WorkerContext CreateWorkerContext() =>
            new WorkerContext(_width, _height, _availablePieces.Count, _totalPieceSlots,
                _initialOccupied, _initialTileArray, _lockedPlacements,
                _emotionRules, _completionRuleConfigs, _clonedZones, _aspectSources, _blockedList,
                _totalAvailableTiles);

        // Binary search: first index in sorted arr where anchor is lex-greater than minAnchor.
        private static int FindFirstGreater(PrecomputedPlacement[] arr, Vector2Int minAnchor)
        {
            int lo = 0, hi = arr.Length;
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (IsLexGreater(arr[mid].Anchor, minAnchor))
                    hi = mid;
                else
                    lo = mid + 1;
            }
            return lo;
        }

        private bool HasUnplacedLowerIdentical(int pieceIdx, bool[] pieceUsed)
        {
            foreach (var j in _lowerIdenticalNeighbors[pieceIdx])
                if (!pieceUsed[j]) return true;
            return false;
        }

        // Flag 1 — forward-checking: every unplaced "active" piece must still have at least one
        // valid placement with anchor strictly greater than nextMinAnchor.
        private bool PassesForwardCheck(WorkerContext ctx, Vector2Int nextMinAnchor)
        {
            for (int p = 0; p < _availablePieces.Count; p++)
            {
                if (ctx.PieceUsed[p]) continue;
                if (HasUnplacedLowerIdentical(p, ctx.PieceUsed)) continue; // not the canonical representative yet
                if (!HasValidPlacementAfter(p, ctx.Occupied, nextMinAnchor)) return false;
            }
            return true;
        }

        private bool HasValidPlacementAfter(int pieceIdx, bool[] occupied, Vector2Int minAnchor)
        {
            var arr = _precomputed[pieceIdx];
            int start = FindFirstGreater(arr, minAnchor);
            for (int j = start; j < arr.Length; j++)
                if (!OccupancyConflict(arr[j].TileIndices, occupied)) return true;
            return false;
        }

        // Flag 3 — full-coverage component check: BFS all remaining empty cells into connected
        // components; each component's size must be expressible as a sum of some subset of
        // remaining piece tile counts (0/1 knapsack DP). Returns false → prune this branch.
        private bool PassesFullCoverageCheck(WorkerContext ctx)
        {
            // Build 0/1 achievability set from remaining piece tile counts
            var ach = ctx.AchievableBuffer;
            Array.Clear(ach, 0, _totalAvailableTiles + 1);
            ach[0] = true;
            for (int p = 0; p < _availablePieces.Count; p++)
            {
                if (ctx.PieceUsed[p]) continue;
                int t = _availablePieceTileCounts[p];
                for (int s = _totalAvailableTiles; s >= t; s--)
                    if (ach[s - t]) ach[s] = true;
            }

            // BFS connected components of empty (non-occupied, non-blocked) cells
            int boardSize = _width * _height;
            Array.Clear(ctx.Visited, 0, boardSize);
            int qHead, qTail;

            for (int startCell = 0; startCell < boardSize; startCell++)
            {
                if (ctx.Occupied[startCell] || _blockedFlat[startCell] || ctx.Visited[startCell])
                    continue;

                qHead = 0; qTail = 0;
                ctx.BfsQueue[qTail++] = startCell;
                ctx.Visited[startCell] = true;
                int compSize = 0;

                while (qHead < qTail)
                {
                    int idx = ctx.BfsQueue[qHead++];
                    compSize++;
                    int x = idx % _width, y = idx / _width;
                    int nb;
                    if (x > 0           && !ctx.Occupied[nb = idx - 1]      && !_blockedFlat[nb] && !ctx.Visited[nb]) { ctx.Visited[nb] = true; ctx.BfsQueue[qTail++] = nb; }
                    if (x < _width - 1  && !ctx.Occupied[nb = idx + 1]      && !_blockedFlat[nb] && !ctx.Visited[nb]) { ctx.Visited[nb] = true; ctx.BfsQueue[qTail++] = nb; }
                    if (y > 0           && !ctx.Occupied[nb = idx - _width]  && !_blockedFlat[nb] && !ctx.Visited[nb]) { ctx.Visited[nb] = true; ctx.BfsQueue[qTail++] = nb; }
                    if (y < _height - 1 && !ctx.Occupied[nb = idx + _width]  && !_blockedFlat[nb] && !ctx.Visited[nb]) { ctx.Visited[nb] = true; ctx.BfsQueue[qTail++] = nb; }
                }

                if (!ach[compSize]) return false;
            }
            return true;
        }

        private static bool OccupancyConflict(int[] tileIndices, bool[] occupied)
        {
            foreach (var idx in tileIndices)
                if (occupied[idx]) return true;
            return false;
        }

        private bool IsStaticallyValid(Vector2Int[] tiles, HashSet<Vector2Int> blockedSet)
        {
            foreach (var tile in tiles)
            {
                if (tile.x < 0 || tile.x >= _width || tile.y < 0 || tile.y >= _height) return false;
                if (blockedSet.Contains(tile)) return false;
                if (_initialOccupied[tile.y * _width + tile.x]) return false;
            }

            // Reject placements that span a wall — mirrors BoardController.CrossesWall
            if (_horizontalWallSet.Count > 0 || _verticalWallSet.Count > 0)
            {
                var tileSet = new HashSet<Vector2Int>(tiles);
                foreach (var t in tiles)
                {
                    if (_horizontalWallSet.Contains(t) && tileSet.Contains(new Vector2Int(t.x, t.y + 1)))
                        return false;
                    if (_verticalWallSet.Contains(t) && tileSet.Contains(new Vector2Int(t.x + 1, t.y)))
                        return false;
                }
            }

            return true;
        }

        private static Vector2Int ComputeAnchor(Vector2Int[] tiles)
        {
            var anchor = tiles[0];
            for (int i = 1; i < tiles.Length; i++)
            {
                var t = tiles[i];
                if (t.y < anchor.y || (t.y == anchor.y && t.x < anchor.x))
                    anchor = t;
            }
            return anchor;
        }

        // Lex order: primary y ascending, secondary x ascending (row-major)
        private static bool IsLexGreater(Vector2Int a, Vector2Int b)
            => a.y > b.y || (a.y == b.y && a.x > b.x);

        private static int LexCompare(Vector2Int a, Vector2Int b)
        {
            var dy = a.y.CompareTo(b.y);
            return dy != 0 ? dy : a.x.CompareTo(b.x);
        }

        private static Vector2Int[] ComputeTilesArray(List<Vector2Int> shape, int rotation, Vector2Int position)
        {
            var tiles = new Vector2Int[shape.Count];
            for (int i = 0; i < shape.Count; i++)
            {
                var p = shape[i];
                Vector2Int rotated = rotation switch
                {
                    0 => p,
                    1 => new Vector2Int(-p.y, p.x),
                    2 => new Vector2Int(-p.x, -p.y),
                    _ => new Vector2Int(p.y, -p.x)
                };
                tiles[i] = rotated + position;
            }
            return tiles;
        }

        private static List<int> ComputeUniqueRotations(List<Vector2Int> shape)
        {
            var allNormalized = ShapeHelper.GetAllNormalizedRotations(shape);
            var unique = new List<int>();
            for (int r = 0; r < 4; r++)
            {
                bool dup = false;
                for (int prev = 0; prev < r; prev++)
                    if (ShapeHelper.AreShapesEqual(allNormalized[r], allNormalized[prev]))
                    { dup = true; break; }
                if (!dup) unique.Add(r);
            }
            return unique;
        }
    }
}
