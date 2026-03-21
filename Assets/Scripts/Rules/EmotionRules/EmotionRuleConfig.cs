using System;

namespace Rules.EmotionRules
{
    [Serializable]
    public class EmotionRuleConfig
    {
        public EmotionRuleSO rule;
        [UnityEngine.SerializeReference] public EmotionRuleArgs args;
    }
}
