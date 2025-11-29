using Core;

namespace Rules
{
    public class RuleContext
    {
        public GameState State { get; }
        public Piece.Piece[,] TileArray { get; }

        public RuleContext(GameState gameState, Piece.Piece[,] tileArray)
        {
            State = gameState;
            TileArray = tileArray;
        }
    }
}