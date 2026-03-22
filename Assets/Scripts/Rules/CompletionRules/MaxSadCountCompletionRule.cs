using System;
using Core;

namespace Rules.CompletionRules
{
    [Serializable]
    public class MaxSadCountCompletionRule : CompletionRule
    {
        public int maximumSadPieces;

        public override bool IsMet(EmotionEvaluationResult result, GameState state)
            => result.SadCount <= maximumSadPieces;

        public override string GetDescription()
            => $"At most {maximumSadPieces} piece(s) may be sad";

        public override string GetProgress(EmotionEvaluationResult result, GameState state)
            => $"{result.SadCount}/{maximumSadPieces} sad";
    }
}
