using System.Collections.Generic;
using UnityEngine;

namespace Piece
{
    public class Shape
    {
        public List<Vector2Int> tilePosition;

        public Shape(List<Vector2Int> tilePosition)
        {
            this.tilePosition = tilePosition;
        }

        public Shape(ShapeSO shapeSO) : this(shapeSO.tilePosition)
        {
            
        }
    }
}