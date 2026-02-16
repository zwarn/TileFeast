using System;
using System.Collections.Generic;
using System.Linq;
using Rules;
using Zones.Rules;
using Sirenix.Serialization;
using UnityEngine;

namespace Zones
{
    [Serializable]
    public class Zone
    {
        [SerializeReference] public ZoneSO zoneType;
        public List<Vector2Int> positions;
        [SerializeReference] [OdinSerialize] public ZoneRule zoneRule;

        public Zone(ZoneSO zoneType, List<Vector2Int> positions)
        {
            this.zoneType = zoneType;
            this.positions = positions;
            zoneRule = zoneType.zoneRule;
        }

        public bool IsSatisfied()
        {
            if (zoneRule == null) return true;

            return zoneRule.IsSatisfied();
        }

        public int GetScore()
        {
            if (zoneRule == null) return 0;

            return zoneRule.GetScore();
        }

        public void Calculate(RuleContext context)
        {
            if (zoneRule == null)
            {
                return;
            }

            zoneRule.Calculate(new ZoneContext(positions, context.State, context.TileArray));
        }

        public Zone Clone()
        {
            return new Zone(zoneType, positions.ToList());
        }
    }
}