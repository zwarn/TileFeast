using Core;
using Piece;

namespace Rules.Placement
{
    public class PlacementRuleContext
    {
        public GameState State { get; }
        public PieceSO[,] TileArray { get; }

        public PlacementRuleContext(GameState gameState, PieceSO[,] tileArray)
        {
            State = gameState;
            TileArray = tileArray;
        }
    }
}