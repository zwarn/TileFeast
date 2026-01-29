using System;
using System.Collections.Generic;
using System.Linq;
using Rules;
using Rules.ZoneRules;
using Sirenix.Serialization;
using UnityEngine;

namespace Board.Zone
{
    [Serializable]
    public class Zone
    {
        [SerializeReference] public ZoneSO zoneType;
        public List<Vector2Int> positions;
        [SerializeReference] [OdinSerialize] public ZoneRule ZoneRule;

        public Zone(ZoneSO zoneType, List<Vector2Int> positions, ZoneRule zoneRule)
        {
            this.zoneType = zoneType;
            this.positions = positions;
            ZoneRule = zoneRule;
        }

        public bool IsSatisfied()
        {
            if (ZoneRule == null) return true;

            return ZoneRule.IsSatisfied();
        }

        public int GetScore()
        {
            if (ZoneRule == null) return 0;

            return ZoneRule.GetScore();
        }

        public void Calculate(RuleContext context)
        {
            if (ZoneRule == null)
            {
                return;
            }

            ZoneRule.Calculate(new ZoneContext(positions, context.State, context.TileArray));
        }

        public Zone Clone()
        {
            return new Zone(zoneType, positions.ToList(), ZoneRule);
        }
    }
}