using Core;
using Pieces;

namespace Rules
{
    public class RuleContext
    {
        public GameState State { get; }
        public Piece[,] TileArray { get; }

        public RuleContext(GameState gameState, Piece[,] tileArray)
        {
            State = gameState;
            TileArray = tileArray;
        }
    }
}