using System.Collections.Generic;
using UnityEngine;
using Zones;

namespace Placeables.BoardExpansions
{
    public class BoardExpansionData
    {
        public List<Vector2Int> HorizontalWalls = new();
        public List<Vector2Int> Shape;
        public List<Vector2Int> VerticalWalls = new();
        public List<Zone> Zones = new();

        public BoardExpansionData(List<Vector2Int> shape)
        {
            Shape = shape;
        }
    }
}