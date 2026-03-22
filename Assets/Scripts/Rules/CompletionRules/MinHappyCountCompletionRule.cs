using System;
using Core;

namespace Rules.CompletionRules
{
    [Serializable]
    public class MinHappyCountCompletionRule : CompletionRule
    {
        public int minimumHappyPieces;

        public override bool IsMet(EmotionEvaluationResult result, GameState state)
            => result.HappyCount >= minimumHappyPieces;

        public override string GetDescription()
            => $"At least {minimumHappyPieces} piece(s) must be happy";

        public override string GetProgress(EmotionEvaluationResult result, GameState state)
            => $"{result.HappyCount}/{minimumHappyPieces} happy";
    }
}
