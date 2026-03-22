using System;
using Core;

namespace Rules.CompletionRules
{
    [Serializable]
    public abstract class CompletionRule
    {
        public abstract bool IsMet(EmotionEvaluationResult result, GameState state);
        public abstract string GetDescription();
        public virtual string GetProgress(EmotionEvaluationResult result, GameState state)
            => IsMet(result, state) ? "Done" : "Not yet";
    }
}
