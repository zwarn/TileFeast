using System;

namespace Rules.EmotionRules
{
    [Serializable]
    public class EmotionRuleConfig
    {
        [UnityEngine.SerializeReference] public EmotionRule rule;
    }
}
