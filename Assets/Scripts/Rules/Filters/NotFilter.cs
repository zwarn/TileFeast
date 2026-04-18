using System;
using Pieces;

namespace Rules.Filters
{
    [Serializable]
    public class NotFilter : PieceFilter
    {
        [UnityEngine.SerializeReference]
        [UnityEngine.Tooltip("Filter to invert")]
        public PieceFilter inner;

        public override bool Matches(PlacedPiece piece, EmotionContext context)
        {
            if (inner == null) return false;
            return !inner.Matches(piece, context);
        }

        public override string GetDescription()
        {
            var innerText = inner != null ? inner.GetDescription() : "?";
            return $"pieces that are not ({innerText})";
        }
    }
}
