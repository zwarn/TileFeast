using System;
using System.Collections.Generic;
using System.Linq;
using Pieces;
using Pieces.Aspects;
using UnityEngine;

namespace Rules.Checks
{
    /// <summary>
    /// Passes when the piece is part of the single largest connected group for the aspect.
    /// Ties: every group tied for the largest counts as a pass.
    /// </summary>
    [Serializable]
    public class LargestAspectGroupMembershipCheck : EmotionCheck
    {
        [UnityEngine.Tooltip("Aspect that defines the connected group")]
        public AspectSO groupAspect;

        public override CheckResult Evaluate(PlacedPiece piece, EmotionContext context)
        {
            if (groupAspect == null) return new CheckResult(false);
            var aspect = new Aspect(groupAspect);
            if (!piece.AllAspects.Contains(aspect))
                return new CheckResult(false);

            var groups = RulesHelper.GetGroups(context.TileArray,
                p => p != null && p.AllAspects.Contains(aspect));
            if (groups.Count == 0) return new CheckResult(false);

            int largest = groups.Max(g => g.Count);
            var pieceTiles = piece.GetTilePosition();
            var myGroup = groups.FirstOrDefault(g => pieceTiles.Any(t => g.Contains(t)));
            int size = myGroup?.Count ?? 0;
            bool passed = size == largest;
            return new CheckResult(passed, $"{groupAspect.name} group of size {size} (largest is {largest})");
        }

        public override string GetDescription()
        {
            var aspectName = groupAspect != null ? groupAspect.name : "?";
            return $"in the largest {aspectName} group";
        }

        public override List<HighlightGroup> GetContextHighlight(
            IEnumerable<PlacedPiece> filteredPieces, EmotionContext context)
        {
            if (groupAspect == null) return new List<HighlightGroup>();
            var aspect = new Aspect(groupAspect);
            var groups = RulesHelper.GetGroups(context.TileArray,
                p => p != null && p.AllAspects.Contains(aspect));
            if (groups.Count == 0) return new List<HighlightGroup>();

            int largest = groups.Max(g => g.Count);
            var largestPositions = groups.Where(g => g.Count == largest).SelectMany(g => g).ToList();
            var smallerPositions = groups.Where(g => g.Count < largest).SelectMany(g => g).ToList();

            var result = new List<HighlightGroup> { new(new Color(0f, 0.9f, 0.9f, 0.75f), largestPositions) };
            if (smallerPositions.Count > 0)
                result.Add(new HighlightGroup(new Color(0.4f, 0.5f, 0.8f, 0.6f), smallerPositions));
            return result;
        }
    }
}
