using System.Collections.Generic;
using UnityEngine;
using Zones;

namespace Placeables.ZonePlacementS
{
    public class ZonePlacementData
    {
        public List<Vector2Int> Shape;
        public ZoneSO ZoneType;

        public ZonePlacementData(ZoneSO zoneType, List<Vector2Int> shape)
        {
            ZoneType = zoneType;
            Shape = shape;
        }
    }
}