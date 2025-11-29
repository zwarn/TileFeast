using System;
using System.Collections.Generic;
using Rules;
using Rules.ZoneRules;
using UnityEngine;

namespace Board.Zone
{
    [Serializable]
    public class Zone
    {
        [SerializeReference] public ZoneSO zoneType;
        public List<Vector2Int> positions;

        public bool IsSatisfied()
        {
            if (zoneType.zoneRuleSO == null) return true;

            return zoneType.zoneRuleSO.IsSatisfied();
        }

        public int GetScore()
        {
            if (zoneType.zoneRuleSO == null) return 0;

            return zoneType.zoneRuleSO.GetScore();
        }

        public void Calculate(RuleContext context)
        {
            if (zoneType.zoneRuleSO == null)
            {
                return;
            }

            zoneType.zoneRuleSO.Calculate(new ZoneContext(positions, context.State, context.TileArray));
        }
    }
}