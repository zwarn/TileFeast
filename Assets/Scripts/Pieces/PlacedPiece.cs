using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pieces
{
    public class PlacedPiece
    {
        public PlacedPiece(Piece piece, int rotation, Vector2Int position)
        {
            Piece = piece;
            Rotation = rotation;
            Position = position;
        }

        public int Rotation { get; }

        public Vector2Int Position { get; private set; }
        public Piece Piece { get; }

        public List<Vector2Int> GetTilePosition()
        {
            return Piece.shape.Select(pos =>
            {
                switch (Rotation)
                {
                    case 0: return pos;
                    case 1: return new Vector2Int(-pos.y, pos.x);
                    case 2: return new Vector2Int(-pos.x, -pos.y);
                    default: return new Vector2Int(pos.y, -pos.x);
                }
            }).Select(rotatedPos => rotatedPos += Position).ToList();
        }

        public void Lock(bool locked)
        {
            Piece.locked = locked;
        }

        public bool IsLocked()
        {
            return Piece.locked;
        }

        public void Move(Vector2Int translate)
        {
            Position += translate;
        }
    }
}