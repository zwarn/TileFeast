using System;
using Pieces;

namespace Rules.Filters
{
    /// <summary>
    /// Decides which pieces a rule or aspect source applies to.
    /// GetDescription returns a noun phrase, e.g. "Red pieces" or "all pieces".
    /// </summary>
    [Serializable]
    public abstract class PieceFilter
    {
        public abstract bool Matches(PlacedPiece piece, EmotionContext context);
        public abstract string GetDescription();
    }
}
