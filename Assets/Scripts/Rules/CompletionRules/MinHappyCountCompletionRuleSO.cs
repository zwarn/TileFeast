using Core;
using UnityEngine;

namespace Rules.CompletionRules
{
    [CreateAssetMenu(menuName = "CompletionRule/MinHappyCount", fileName = "MinHappyCountCompletionRule")]
    public class MinHappyCountCompletionRuleSO : CompletionRuleSO
    {
        public override bool IsMet(EmotionEvaluationResult result, GameState state, CompletionRuleArgs args)
        {
            var a = (MinHappyArgs)args;
            return result.HappyCount >= a.minimumHappyPieces;
        }

        public override string GetDescription(CompletionRuleArgs args)
        {
            var a = (MinHappyArgs)args;
            return $"At least {a.minimumHappyPieces} piece(s) must be happy";
        }

        public override string GetProgress(EmotionEvaluationResult result, GameState state, CompletionRuleArgs args)
        {
            var a = (MinHappyArgs)args;
            return $"{result.HappyCount}/{a.minimumHappyPieces} happy";
        }
    }
}
