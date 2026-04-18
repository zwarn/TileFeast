using System;
using System.Linq;
using Pieces;
using Rules.Components;

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
    }
}
