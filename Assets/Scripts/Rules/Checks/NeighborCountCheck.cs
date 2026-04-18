using System;
using System.Linq;
using Pieces;
using Rules.Components;
using Rules.Filters;

namespace Rules.Checks
{
    /// <summary>
    /// Counts neighbors matching an optional filter and tests the count against a range.
    /// Mode selects whether "neighbor" means distinct neighbor pieces or the number of
    /// adjacent tile positions they cover.
    /// Leaving <see cref="neighborFilter"/> null counts every neighbor.
    /// </summary>
    [Serializable]
    public class NeighborCountCheck : EmotionCheck
    {
        [UnityEngine.SerializeReference]
        [UnityEngine.Tooltip("Neighbor must match this filter. Leave null to count any neighbor.")]
        public PieceFilter neighborFilter;

        [UnityEngine.Tooltip("Allowed count of matching neighbors")]
        public IntRange countRange = new IntRange { min = 1, max = -1 };

        [UnityEngine.Tooltip("DistinctPieces: each neighbor piece counts once. CoveredTiles: each adjacent tile counts.")]
        public NeighborCountMode mode = NeighborCountMode.DistinctPieces;

        public override CheckResult Evaluate(PlacedPiece piece, EmotionContext context)
        {
            int count = CountMatches(piece, context);
            bool passed = countRange.Contains(count);
            return new CheckResult(passed, BuildDetailReason(count));
        }

        public override string GetDescription()
        {
            var range = countRange != null ? countRange.GetDescription() : "any";
            var subject = SubjectText();
            return mode == NeighborCountMode.CoveredTiles
                ? $"next to {range} adjacent tile(s) of {subject}"
                : $"next to {range} {subject}";
        }

        private int CountMatches(PlacedPiece piece, EmotionContext context)
        {
            if (mode == NeighborCountMode.CoveredTiles)
            {
                return RulesHelper.GetNeighborPositions(piece, context.TileArray)
                    .Count(pos =>
                    {
                        var tile = context.TileArray[pos.x, pos.y];
                        if (tile == null) return false;
                        return neighborFilter == null || neighborFilter.Matches(tile, context);
                    });
            }

            var neighbors = RulesHelper.GetNeighborPieces(piece, context.TileArray);
            if (neighborFilter == null) return neighbors.Count;
            return neighbors.Count(n => neighborFilter.Matches(n, context));
        }

        private string SubjectText()
        {
            if (neighborFilter == null) return "neighboring piece(s)";
            return neighborFilter.GetDescription();
        }

        private string BuildDetailReason(int count)
        {
            var subject = SubjectText();
            return mode == NeighborCountMode.CoveredTiles
                ? $"{count} adjacent tile(s) of {subject}"
                : $"{count} adjacent {subject}";
        }
    }
}
