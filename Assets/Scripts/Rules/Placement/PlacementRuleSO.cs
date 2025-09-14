using System.Collections.Generic;
using UnityEngine;

namespace Rules.Placement
{
    public abstract class PlacementRuleSO : ScriptableObject
    {
        public abstract bool IsSatisfied();
        public abstract void Calculate(PlacementRuleContext ruleContext);

        public abstract List<Vector2Int> GetViolationSpots();

        public abstract string GetText();
    }
}