using Core;
using Piece;

namespace Rules.Placement
{
    public class PlacementRuleContext
    {
        public GameState State { get; }
        public Piece.Piece[,] TileArray { get; }

        public PlacementRuleContext(GameState gameState, Piece.Piece[,] tileArray)
        {
            State = gameState;
            TileArray = tileArray;
        }
    }
}