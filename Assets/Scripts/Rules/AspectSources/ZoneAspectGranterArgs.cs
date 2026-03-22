using System;
using Pieces;
using Pieces.Aspects;
using Zones;

namespace Rules.AspectSources
{
    [Serializable]
    public class ZoneAspectGranterArgs : AspectSourceArgs
    {
        public ZoneSO targetZone;
        public AspectSO grantedAspect;
        public bool includeAdjacent;
    }
}
