using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shape.model
{
    public class PlacedShape
    {
        public PlacedShape(ShapeSO shape, int rotation, Vector2Int position)
        {
            Shape = shape;
            Rotation = rotation;
            Position = position;
        }

        public int Rotation { get; private set; }

        public Vector2Int Position { get; }
        public ShapeSO Shape { get; private set; }

        public List<Vector2Int> GetTilePosition()
        {
            return Shape.tilePosition.Select(pos =>
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