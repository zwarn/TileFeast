using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Pieces;
using Rules;
using Rules.PlacementRules;
using Rules.ScoreRules;
using Scenarios;
using UnityEngine;
using Zones;

namespace Solver
{
    /// <summary>
    /// Exhaustive backtracking solver. Construct on the main thread (clones ScriptableObjects),
    /// then call SolveAsync to run on a background thread.
    /// </summary>
    public class AutoSolver
    {
        // Progress counters — written by background thread, read by main thread.
        // FoundCount is volatile (int = 32-bit, volatile is valid).
        // TriedCount is long (64-bit); volatile is not valid for long in C#,
        // so callers must use Interlocked.Read(ref solver.TriedCount) for a safe cross-thread read.
        public long TriedCount;
        public volatile int FoundCount;

        private readonly int _width;
        private readonly int _height;
        private readonly HashSet<Vector2Int> _blockedPositions;
        private readonly List<Piece> _availablePieces;
        private readonly int _maxResults;

        // Cloned rule instances (created on main thread, used on background thread)
        private readonly List<PlacementRuleSO> _clonedPlacementRules;
        private readonly List<ScoreRuleSO> _clonedScoreRules;
        private readonly List<Zone> _clonedZones;

        // Precomputed unique rotation indices per piece (indexed by _availablePieces index)
        private readonly List<int>[] _uniqueRotations;

        // Backtracking board state (used only on background thread)
        private readonly HashSet<Vector2Int> _occupied = new();
        private readonly Dictionary<Vector2Int, PlacedPiece> _pieceDict = new();
        private readonly List<PlacedPiece> _currentPlacements = new();

        // Results
        private readonly List<SolverResult> _results = new();
        private readonly object _resultsLock = new();

        public AutoSolver(ScenarioSO scenario, int maxResults)
        {
            _maxResults = maxResults;
            _width = scenario.gridSize.x;
            _height = scenario.gridSize.y;
            _blockedPositions = new HashSet<Vector2Int>(scenario.blockedPositions);
            _availablePieces = scenario.AvailablePieces();

            // Clone rules on main thread (Object.Instantiate requires main thread)
            _clonedPlacementRules = scenario.placementRules
                .Select(r => UnityEngine.Object.Instantiate(r))
                .ToList();

            _clonedScoreRules = scenario.scoreRules
                .Select(r => UnityEngine.Object.Instantiate(r))
                .ToList();

            _clonedZones = scenario.Zones();

            // Pre-place locked pieces onto the solver board
            foreach (var locked in scenario.LockedPieces())
            {
                _currentPlacements.Add(locked);
                foreach (var tile in locked.GetTilePosition())
                {
                    _occupied.Add(tile);
                    _pieceDict[tile] = locked;
                }
            }

            // Precompute unique rotation indices per piece
            _uniqueRotations = new List<int>[_availablePieces.Count];
            for (int i = 0; i < _availablePieces.Count; i++)
            {
                _uniqueRotations[i] = ComputeUniqueRotations(_availablePieces[i].shape);
            }
        }

        public Task<List<SolverResult>> SolveAsync(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                Backtrack(0, ct);
                return GetResults();
            }, ct);
        }

        public List<SolverResult> GetResults()
        {
            lock (_resultsLock)
            {
                return _results.ToList();
            }
        }

        private void Backtrack(int depth, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;

            if (depth == _availablePieces.Count)
            {
                Interlocked.Increment(ref TriedCount);
                EvaluateTerminal();
                return;
            }

            var piece = _availablePieces[depth];
            var uniqueRotations = _uniqueRotations[depth];

            foreach (int rotation in uniqueRotations)
            {
                if (ct.IsCancellationRequested) return;

                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        var position = new Vector2Int(x, y);
                        var tiles = ComputeTiles(piece.shape, rotation, position);

                        if (!IsValidPlacement(tiles)) continue;

                        // Place
                        var placed = new PlacedPiece(piece, rotation, position);
                        _currentPlacements.Add(placed);
                        foreach (var tile in tiles)
                        {
                            _occupied.Add(tile);
                            _pieceDict[tile] = placed;
                        }

                        Backtrack(depth + 1, ct);

                        // Unplace
                        _currentPlacements.RemoveAt(_currentPlacements.Count - 1);
                        foreach (var tile in tiles)
                        {
                            _occupied.Remove(tile);
                            _pieceDict.Remove(tile);
                        }
                    }
                }
            }
        }

        private void EvaluateTerminal()
        {
            var tileArray = RulesHelper.ConvertTiles(_pieceDict, _width, _height);

            var gameState = new GameState(
                new Vector2Int(_width, _height),
                _blockedPositions.ToList(),
                _currentPlacements.ToList(),
                new List<Piece>(),
                null,
                _clonedScoreRules,
                _clonedPlacementRules,
                _clonedZones
            );

            var context = new RuleContext(gameState, tileArray);

            _clonedPlacementRules.ForEach(r => r.Calculate(context));
            _clonedScoreRules.ForEach(r => r.CalculateScore(context));
            _clonedZones.ForEach(z => z.Calculate(context));

            bool satisfied = _clonedPlacementRules.All(r => r.IsSatisfied())
                          && _clonedZones.All(z => z.IsSatisfied());

            if (!satisfied) return;

            int score = _clonedScoreRules.Sum(r => r.GetScore())
                      + _clonedZones.Sum(z => z.GetScore());

            var result = new SolverResult(_currentPlacements.ToList(), score, true);

            lock (_resultsLock)
            {
                // Insert sorted descending by score
                int insertAt = _results.Count;
                for (int i = 0; i < _results.Count; i++)
                {
                    if (score > _results[i].Score)
                    {
                        insertAt = i;
                        break;
                    }
                }
                _results.Insert(insertAt, result);

                if (_results.Count > _maxResults)
                    _results.RemoveAt(_results.Count - 1);
            }

            Interlocked.Increment(ref FoundCount);
        }

        private bool IsValidPlacement(List<Vector2Int> tiles)
        {
            foreach (var tile in tiles)
            {
                if (tile.x < 0 || tile.x >= _width) return false;
                if (tile.y < 0 || tile.y >= _height) return false;
                if (_blockedPositions.Contains(tile)) return false;
                if (_occupied.Contains(tile)) return false;
            }
            return true;
        }

        private static List<Vector2Int> ComputeTiles(List<Vector2Int> shape, int rotation, Vector2Int position)
        {
            return shape.Select(pos =>
            {
                Vector2Int rotated = rotation switch
                {
                    0 => pos,
                    1 => new Vector2Int(-pos.y, pos.x),
                    2 => new Vector2Int(-pos.x, -pos.y),
                    _ => new Vector2Int(pos.y, -pos.x)
                };
                return rotated + position;
            }).ToList();
        }

        private static List<int> ComputeUniqueRotations(List<Vector2Int> shape)
        {
            var allNormalized = ShapeHelper.GetAllNormalizedRotations(shape);
            var uniqueIndices = new List<int>();

            for (int r = 0; r < 4; r++)
            {
                bool isDuplicate = false;
                for (int prev = 0; prev < r; prev++)
                {
                    if (ShapeHelper.AreShapesEqual(allNormalized[r], allNormalized[prev]))
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                if (!isDuplicate)
                    uniqueIndices.Add(r);
            }

            return uniqueIndices;
        }
    }
}
