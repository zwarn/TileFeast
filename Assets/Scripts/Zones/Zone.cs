using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zones
{
    [Serializable]
    public class Zone
    {
        [SerializeReference] public ZoneSO zoneType;
        public List<Vector2Int> positions;

        public Zone(ZoneSO zoneType, List<Vector2Int> positions)
        {
            this.zoneType = zoneType;
            this.positions = positions;
        }

        public Zone Clone() => new Zone(zoneType, positions.ToList());
    }
}
