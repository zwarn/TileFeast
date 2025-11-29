using UnityEngine;

namespace Rules.ZoneRules
{
    public abstract class ZoneRuleSO : ScriptableObject
    {
        public abstract int GetScore();
        public abstract void Calculate(ZoneContext context);
        public abstract bool IsSatisfied();

        public abstract HighlightData GetHighlight();
        public abstract string GetText();
    }
}