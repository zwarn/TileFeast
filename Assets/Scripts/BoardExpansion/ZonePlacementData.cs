using System.Collections.Generic;
using UnityEngine;
using Zones;

namespace BoardExpansion
{
    public class ZonePlacementData
    {
        public ZoneSO ZoneType;
        public List<Vector2Int> Shape;

        public ZonePlacementData(ZoneSO zoneType, List<Vector2Int> shape)
        {
            ZoneType = zoneType;
            Shape = shape;
        }
    }
}
