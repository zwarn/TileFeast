using System;

namespace Rules.CompletionRules
{
    [Serializable]
    public class CompletionRuleConfig
    {
        [UnityEngine.SerializeReference] public CompletionRule rule;
    }
}
