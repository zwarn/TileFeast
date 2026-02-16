using System;
using Rules;

namespace Zones.Rules
{
    [Serializable]
    public abstract class ZoneRule
    {
        public abstract int GetScore();
        public abstract void Calculate(ZoneContext context);
        public abstract bool IsSatisfied();

        public abstract HighlightData GetHighlight();
        public abstract string GetText();
    }
}