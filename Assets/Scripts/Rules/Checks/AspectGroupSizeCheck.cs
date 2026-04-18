using System;
using System.Linq;
using Pieces;
using Pieces.Aspects;
using Rules.Components;

namespace Rules.Checks
{
    /// <summary>
    /// Passes when the piece participates in a flood-fill group (defined by
    /// <see cref="groupAspect"/>) whose size is within <see cref="sizeRange"/>.
    /// If the piece itself does not carry the group aspect the check fails.
    /// </summary>
    [Serializable]
    public class AspectGroupSizeCheck : EmotionCheck
    {
        [UnityEngine.Tooltip("Aspect that defines the connected group")]
        public AspectSO groupAspect;

        [UnityEngine.Tooltip("Allowed group size range")]
        public IntRange sizeRange = new IntRange { min = 1, max = -1 };

        public override CheckResult Evaluate(PlacedPiece piece, EmotionContext context)
        {
            if (groupAspect == null) return new CheckResult(false);
            var aspect = new Aspect(groupAspect);
            if (!piece.AllAspects.Contains(aspect))
                return new CheckResult(false, $"not part of a {groupAspect.name} group");

            var groups = RulesHelper.GetGroups(context.TileArray,
                p => p != null && p.AllAspects.Contains(aspect));
            var pieceTiles = piece.GetTilePosition();
            var myGroup = groups.FirstOrDefault(g => pieceTiles.Any(t => g.Contains(t)));
            int size = myGroup?.Count ?? 0;
            bool passed = sizeRange.Contains(size);
            return new CheckResult(passed, $"{groupAspect.name} group of size {size}");
        }

        public override string GetDescription()
        {
            var aspectName = groupAspect != null ? groupAspect.name : "?";
            var range = sizeRange != null ? sizeRange.GetDescription() : "any";
            return $"in a {aspectName} group of size {range}";
        }
    }
}
