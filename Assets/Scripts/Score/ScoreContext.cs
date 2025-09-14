using Core;
using Piece;

namespace Score
{
    public class ScoreContext
    {
        public GameState State { get; }
        public PieceSO[,] TileArray { get; }

        public ScoreContext(GameState gameState, PieceSO[,] tileArray)
        {
            State = gameState;
            TileArray = tileArray;
        }
    }
}