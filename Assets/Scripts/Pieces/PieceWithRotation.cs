namespace Pieces
{
    public class PieceWithRotation
    {
        public PieceWithRotation(Piece piece, int rotation)
        {
            Piece = piece;
            Rotation = rotation;
        }

        public int Rotation { get; private set; }

        public Piece Piece { get; private set; }

        public void Rotate(int dir)
        {
            var rotation = Rotation + dir;
            if (rotation < 0) rotation += 4;

            if (rotation > 3) rotation -= 4;

            Rotation = rotation;
        }
    }
}