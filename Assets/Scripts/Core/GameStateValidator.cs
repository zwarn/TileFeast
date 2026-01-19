using System.Collections.Generic;
using System.Linq;
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
            ValidatePlacedPieces(state, result);

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
            }
        }

        private static bool IsWithinBounds(Vector2Int position, Vector2Int gridSize)
        {
            return position.x >= 0 && position.x < gridSize.x &&
                   position.y >= 0 && position.y < gridSize.y;
        }
    }
}
