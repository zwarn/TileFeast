using System.Collections.Generic;
using System.Linq;
using Rules.AspectSources;
using Rules.CompletionRules;
using Rules.EmotionRules;
using UnityEngine;

namespace Core
{
    public class GameStateValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new();

        public void AddError(string error)
        {
            Errors.Add(error);
        }
    }

    public static class GameStateValidator
    {
        public static GameStateValidationResult Validate(GameState state)
        {
            var result = new GameStateValidationResult();

            ValidateGridSize(state, result);
            ValidateBlockedPositions(state, result);
            ValidateHorizontalWalls(state, result);
            ValidateVerticalWalls(state, result);
            ValidatePlacedPieces(state, result);
            ValidateZones(state, result);
            ValidateRules(state, result);

            return result;
        }

        private static void ValidateGridSize(GameState state, GameStateValidationResult result)
        {
            if (state.GridSize.x <= 0 || state.GridSize.y <= 0)
            {
                result.AddError($"Grid size must be positive. Got: {state.GridSize}");
            }
        }

        private static void ValidateBlockedPositions(GameState state, GameStateValidationResult result)
        {
            if (state.BlockedPositions == null) return;

            foreach (var pos in state.BlockedPositions)
            {
                if (!IsWithinBounds(pos, state.GridSize))
                {
                    result.AddError($"Blocked position {pos} is outside grid bounds {state.GridSize}");
                }
            }
        }

        private static void ValidateHorizontalWalls(GameState state, GameStateValidationResult result)
        {
            if (state.HorizontalWalls == null) return;

            foreach (var wall in state.HorizontalWalls)
            {
                if (wall.x < 0 || wall.x >= state.GridSize.x ||
                    wall.y < 0 || wall.y >= state.GridSize.y - 1)
                {
                    result.AddError($"Horizontal wall {wall} is outside valid range for grid {state.GridSize}");
                }
            }
        }

        private static void ValidateVerticalWalls(GameState state, GameStateValidationResult result)
        {
            if (state.VerticalWalls == null) return;

            foreach (var wall in state.VerticalWalls)
            {
                if (wall.x < 0 || wall.x >= state.GridSize.x - 1 ||
                    wall.y < 0 || wall.y >= state.GridSize.y)
                {
                    result.AddError($"Vertical wall {wall} is outside valid range for grid {state.GridSize}");
                }
            }
        }

        private static void ValidatePlacedPieces(GameState state, GameStateValidationResult result)
        {
            if (state.PlacedPieces == null) return;

            var occupiedPositions = new HashSet<Vector2Int>();
            var blockedSet = new HashSet<Vector2Int>(state.BlockedPositions ?? new List<Vector2Int>());

            foreach (var placedPiece in state.PlacedPieces)
            {
                var tilePositions = placedPiece.GetTilePosition();

                foreach (var tilePos in tilePositions)
                {
                    if (!IsWithinBounds(tilePos, state.GridSize))
                    {
                        result.AddError($"Piece at {placedPiece.Position} has tile outside grid bounds at {tilePos}");
                    }

                    if (blockedSet.Contains(tilePos))
                    {
                        result.AddError($"Piece at {placedPiece.Position} overlaps blocked position at {tilePos}");
                    }

                    if (!occupiedPositions.Add(tilePos))
                    {
                        result.AddError($"Multiple pieces overlap at position {tilePos}");
                    }
                }

                var tileSet = new HashSet<Vector2Int>(tilePositions);
                if (state.HorizontalWalls != null)
                {
                    foreach (var wall in state.HorizontalWalls)
                    {
                        if (tileSet.Contains(wall) && tileSet.Contains(new Vector2Int(wall.x, wall.y + 1)))
                            result.AddError($"Piece at {placedPiece.Position} crosses horizontal wall at {wall}");
                    }
                }
                if (state.VerticalWalls != null)
                {
                    foreach (var wall in state.VerticalWalls)
                    {
                        if (tileSet.Contains(wall) && tileSet.Contains(new Vector2Int(wall.x + 1, wall.y)))
                            result.AddError($"Piece at {placedPiece.Position} crosses vertical wall at {wall}");
                    }
                }
            }
        }

        private static void ValidateZones(GameState state, GameStateValidationResult result)
        {
            if (state.Zones == null) return;

            var allZonePositions = new HashSet<Vector2Int>();

            for (var i = 0; i < state.Zones.Count; i++)
            {
                var zone = state.Zones[i];
                var seenInZone = new HashSet<Vector2Int>();

                foreach (var pos in zone.positions)
                {
                    if (!IsWithinBounds(pos, state.GridSize))
                    {
                        result.AddError($"Zone {i} has position {pos} outside grid bounds {state.GridSize}");
                    }

                    if (!seenInZone.Add(pos))
                    {
                        result.AddError($"Zone {i} has duplicate position {pos}");
                    }

                    if (!allZonePositions.Add(pos))
                    {
                        result.AddError($"Zone {i} overlaps with another zone at position {pos}");
                    }
                }
            }
        }

        private static void ValidateRules(GameState state, GameStateValidationResult result)
        {
            if (state.EmotionRules != null)
            {
                for (int i = 0; i < state.EmotionRules.Count; i++)
                {
                    var config = state.EmotionRules[i];
                    if (config.rule == null)
                    {
                        result.AddError($"Emotion rule at index {i} has no rule assigned");
                        continue;
                    }

                    if (config.rule is AspectAdjacencyEmotionRule adj)
                    {
                        if (adj.neighborAspect == null)
                            result.AddError($"AspectAdjacencyEmotionRule at index {i}: neighborAspect is not assigned");
                    }
                    else if (config.rule is ZoneProximityEmotionRule zone)
                    {
                        if (zone.targetZoneType == null)
                            result.AddError($"ZoneProximityEmotionRule at index {i}: targetZoneType is not assigned");
                    }
                }
            }

            if (state.CompletionRules != null)
            {
                for (int i = 0; i < state.CompletionRules.Count; i++)
                {
                    var config = state.CompletionRules[i];
                    if (config.rule == null)
                    {
                        result.AddError($"Completion rule at index {i} has no rule assigned");
                        continue;
                    }

                    if (config.rule is MinHappyCountCompletionRule minHappy)
                    {
                        if (minHappy.minimumHappyPieces <= 0)
                            result.AddError($"MinHappyCountCompletionRule at index {i}: minimumHappyPieces must be > 0, got {minHappy.minimumHappyPieces}");
                    }
                    else if (config.rule is MaxSadCountCompletionRule maxSad)
                    {
                        if (maxSad.maximumSadPieces < 0)
                            result.AddError($"MaxSadCountCompletionRule at index {i}: maximumSadPieces must be >= 0, got {maxSad.maximumSadPieces}");
                    }
                }
            }

            if (state.AspectSources != null)
            {
                for (int i = 0; i < state.AspectSources.Count; i++)
                {
                    var config = state.AspectSources[i];
                    if (config.source == null)
                    {
                        result.AddError($"Aspect source at index {i} has no source assigned");
                        continue;
                    }

                    if (config.source is ZoneAspectGranter granter)
                    {
                        if (granter.targetZone == null)
                            result.AddError($"ZoneAspectGranter at index {i}: targetZone is not assigned");
                        if (granter.grantedAspect == null)
                            result.AddError($"ZoneAspectGranter at index {i}: grantedAspect is not assigned");
                    }
                }
            }
        }

        private static bool IsWithinBounds(Vector2Int position, Vector2Int gridSize)
        {
            return position.x >= 0 && position.x < gridSize.x &&
                   position.y >= 0 && position.y < gridSize.y;
        }
    }
}
