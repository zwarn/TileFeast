using System.Collections.Generic;
using UnityEngine;

namespace BoardExpansion
{
    public class WallPlacementData
    {
        public List<Vector2Int> HorizontalWalls = new();
        public List<Vector2Int> VerticalWalls   = new();
    }
}
