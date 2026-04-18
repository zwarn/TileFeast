using System;
using Rules.Checks;
using Rules.EmotionRules;

namespace Rules.Conclusions
{
    /// <summary>
    /// Converts a CheckResult into an EmotionEffect (or null for "no effect").
    /// GetDescription returns an adjective / short phrase, e.g. "Happy" or "Happy, else Sad".
    /// </summary>
    [Serializable]
    public abstract class EmotionConclusion
    {
        public abstract EmotionEffect Build(CheckResult result, EmotionRule source, EmotionCheck check);
        public abstract string GetDescription();
    }
}
