using Core;
using UnityEngine;

namespace Rules.CompletionRules
{
    [CreateAssetMenu(menuName = "CompletionRule/MaxSadCount", fileName = "MaxSadCountCompletionRule")]
    public class MaxSadCountCompletionRuleSO : CompletionRuleSO
    {
        public override bool IsMet(EmotionEvaluationResult result, GameState state, CompletionRuleArgs args)
        {
            var a = (MaxSadArgs)args;
            return result.SadCount <= a.maximumSadPieces;
        }

        public override string GetDescription(CompletionRuleArgs args)
        {
            var a = (MaxSadArgs)args;
            return $"At most {a.maximumSadPieces} piece(s) may be sad";
        }

        public override string GetProgress(EmotionEvaluationResult result, GameState state, CompletionRuleArgs args)
        {
            var a = (MaxSadArgs)args;
            return $"{result.SadCount}/{a.maximumSadPieces} sad";
        }
    }
}
