using System;

namespace Rules.CompletionRules
{
    [Serializable]
    public class CompletionRuleConfig
    {
        public CompletionRuleSO rule;
        [UnityEngine.SerializeReference] public CompletionRuleArgs args;
    }
}
