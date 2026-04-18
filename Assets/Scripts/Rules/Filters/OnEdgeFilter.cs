using System;
using System.Linq;
using Pieces;

namespace Rules.Filters
{
    /// <summary>
    /// Matches pieces that are adjacent to the edge of the board or to a blocked (hole) cell.
    /// </summary>
    [Serializable]
    public class OnEdgeFilter : PieceFilter
    {
        public override bool Matches(PlacedPiece piece, EmotionContext context)
        {
            if (context.TileArray == null) return false;
            var width = context.TileArray.GetLength(0);
            var height = context.TileArray.GetLength(1);
            var blocked = context.State.BlockedPositions;

            return RulesHelper.GetRawNeighborPositions(piece).Any(pos =>
                pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height ||
                blocked.Contains(pos));
        }

        public override string GetDescription() => "pieces touching an edge or hole";
    }
}
