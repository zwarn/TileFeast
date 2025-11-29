using System;
using System.Collections.Generic;
using UnityEngine;

namespace Board.Zone
{
    [Serializable]
    public class Zone
    {
        [SerializeReference] public ZoneSO zoneType;
        public List<Vector2Int> positions;
    }
}