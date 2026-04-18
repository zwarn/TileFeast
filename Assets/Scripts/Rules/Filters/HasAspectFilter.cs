using System;
using System.Linq;
using Pieces;
using Pieces.Aspects;

namespace Rules.Filters
{
    [Serializable]
    public class HasAspectFilter : PieceFilter
    {
        [UnityEngine.Tooltip("The aspect the piece must have")]
        public AspectSO aspect;

        public override bool Matches(PlacedPiece piece, EmotionContext context)
        {
            if (aspect == null) return false;
            return piece.AllAspects.Contains(new Aspect(aspect));
        }

        public override string GetDescription()
        {
            var name = aspect != null ? aspect.name : "?";
            return $"{name} pieces";
        }
    }
}
