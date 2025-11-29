using UnityEngine;

namespace Rules.PlacementRules
{
    public abstract class PlacementRuleSO : ScriptableObject
    {
        public abstract bool IsSatisfied();
        public abstract void Calculate(RuleContext context);

        public abstract HighlightData GetViolationSpots();

        public abstract string GetText();
    }
}