using Core;
using Piece;

namespace Rules.Score
{
    public class ScoreContext
    {
        public GameState State { get; }
        public Piece.Piece[,] TileArray { get; }

        public ScoreContext(GameState gameState, Piece.Piece[,] tileArray)
        {
            State = gameState;
            TileArray = tileArray;
        }
    }
}