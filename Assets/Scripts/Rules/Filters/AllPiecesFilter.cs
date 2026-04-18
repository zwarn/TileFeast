using System;
using Pieces;

namespace Rules.Filters
{
    [Serializable]
    public class AllPiecesFilter : PieceFilter
    {
        public override bool Matches(PlacedPiece piece, EmotionContext context) => true;
        public override string GetDescription() => "All pieces";
    }
}
