using System.Collections.Generic;
using UnityEngine;

namespace Placeables.WallPlacements
{
    public class WallPlacementData
    {
        public List<Vector2Int> HorizontalWalls = new();
        public List<Vector2Int> VerticalWalls = new();
    }
}