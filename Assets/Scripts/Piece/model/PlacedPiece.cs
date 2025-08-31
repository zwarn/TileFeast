using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Piece.model
{
    public class PlacedPiece
    {
        public PlacedPiece(PieceSO piece, int rotation, Vector2Int position)
        {
            Piece = piece;
            Rotation = rotation;
            Position = position;
        }

        public int Rotation { get; private set; }

        public Vector2Int Position { get; }
        public PieceSO Piece { get; private set; }

        public List<Vector2Int> GetTilePosition()
        {
            return Piece.shape.tilePosition.Select(pos =>
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
    }
}