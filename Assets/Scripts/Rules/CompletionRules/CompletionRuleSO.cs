using Core;
using UnityEngine;

namespace Rules.CompletionRules
{
    public abstract class CompletionRuleSO : ScriptableObject
    {
        public abstract bool IsMet(EmotionEvaluationResult result, GameState state, CompletionRuleArgs args);

        public abstract string GetDescription(CompletionRuleArgs args);

        public virtual string GetProgress(EmotionEvaluationResult result, GameState state, CompletionRuleArgs args)
            => IsMet(result, state, args) ? "Done" : "Not yet";
    }
}
