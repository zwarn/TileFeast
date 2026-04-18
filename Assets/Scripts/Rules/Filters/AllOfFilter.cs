using System;
using System.Collections.Generic;
using System.Linq;
using Pieces;

namespace Rules.Filters
{
    [Serializable]
    public class AllOfFilter : PieceFilter
    {
        [UnityEngine.SerializeReference]
        [UnityEngine.Tooltip("Piece must match every filter in this list")]
        public List<PieceFilter> filters = new();

        public override bool Matches(PlacedPiece piece, EmotionContext context)
        {
            if (filters == null || filters.Count == 0) return true;
            return filters.All(f => f != null && f.Matches(piece, context));
        }

        public override string GetDescription()
        {
            if (filters == null || filters.Count == 0) return "all pieces";
            return string.Join(" and ", filters.Select(f => f != null ? $"({f.GetDescription()})" : "(?)"));
        }
    }
}
