using System;
using System.Collections.Generic;
using System.Linq;
using Rules;
using Zones.Rules;
using UnityEngine;

namespace Zones
{
    [Serializable]
    public class Zone
    {
        [SerializeReference] public ZoneSO zoneType;
        public List<Vector2Int> positions;
        [SerializeReference] public ZoneRule zoneRule;

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
            var clone = new Zone(zoneType, positions.ToList());

            // Always clone the zoneRule to ensure each zone has its own instance
            // Use this.zoneRule if available, otherwise fall back to zoneType.zoneRule
            var ruleToClone = zoneRule ?? zoneType?.zoneRule;
            if (ruleToClone != null)
            {
                clone.zoneRule = ruleToClone.Clone();
            }

            return clone;
        }
    }
}