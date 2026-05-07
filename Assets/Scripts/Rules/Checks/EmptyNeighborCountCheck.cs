using System;
using System.Collections.Generic;
using System.Linq;
using Pieces;
using Rules.Components;
using UnityEngine;

namespace Rules.Checks
{
    /// <summary>
    /// Passes when the number of in-bounds, empty (no piece), non-blocked cardinal neighbor
    /// positions is within <see cref="countRange"/>.
    /// </summary>
    [Serializable]
    public class EmptyNeighborCountCheck : EmotionCheck
    {
        [UnityEngine.Tooltip("Allowed count of empty neighboring positions")]
        public IntRange countRange = new IntRange { min = 1, max = -1 };

        public override CheckResult Evaluate(PlacedPiece piece, EmotionContext context)
        {
            var tileArray = context.TileArray;
            var blocked = context.State.BlockedPositions;

            int count = RulesHelper.GetNeighborPositions(piece, tileArray)
                .Count(pos => tileArray[pos.x, pos.y] == null && !blocked.Contains(pos));

            bool passed = countRange.Contains(count);
            return new CheckResult(passed, $"{count} empty neighboring tile(s)");
        }

        public override string GetDescription()
        {
            var range = countRange != null ? countRange.GetDescription() : "any";
            return $"next to {range} empty tile(s)";
        }

        public override List<HighlightGroup> GetContextHighlight(
            IEnumerable<PlacedPiece> filteredPieces, EmotionContext context)
        {
            var tileArray = context.TileArray;
            var blocked   = context.State.BlockedPositions;
            var emptyNeighbors = new HashSet<Vector2Int>();

            foreach (var piece in filteredPieces)
                foreach (var pos in RulesHelper.GetNeighborPositions(piece, tileArray))
                    if (tileArray[pos.x, pos.y] == null && !blocked.Contains(pos))
                        emptyNeighbors.Add(pos);

            if (emptyNeighbors.Count == 0) return new List<HighlightGroup>();
            return new List<HighlightGroup> { new(new Color(1f, 0.85f, 0f, 0.75f), emptyNeighbors.ToList()) };
        }
    }
}
