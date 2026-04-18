using System;
using System.Collections.Generic;
using System.Linq;
using Pieces;

namespace Rules.Filters
{
    [Serializable]
    public class AnyOfFilter : PieceFilter
    {
        [UnityEngine.SerializeReference]
        [UnityEngine.Tooltip("Piece must match at least one filter in this list")]
        public List<PieceFilter> filters = new();

        public override bool Matches(PlacedPiece piece, EmotionContext context)
        {
            if (filters == null || filters.Count == 0) return false;
            return filters.Any(f => f != null && f.Matches(piece, context));
        }

        public override string GetDescription()
        {
            if (filters == null || filters.Count == 0) return "no pieces";
            return string.Join(" or ", filters.Select(f => f != null ? $"({f.GetDescription()})" : "(?)"));
        }
    }
}
