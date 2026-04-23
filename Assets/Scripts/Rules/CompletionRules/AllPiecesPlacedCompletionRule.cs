using System;
using Core;

namespace Rules.CompletionRules
{
    [Serializable]
    public class AllPiecesPlacedCompletionRule : CompletionRule
    {
        public override bool IsMet(EmotionEvaluationResult result, GameState state)
            => !state.HasPieceInHand && state.AvailablePieces.Count == 0;

        public override string GetDescription()
            => "All pieces must be placed on the board";

        public override string GetProgress(EmotionEvaluationResult result, GameState state)
        {
            int remaining = state.AvailablePieces.Count + (state.HasPieceInHand ? 1 : 0);
            return remaining == 0 ? "All placed" : $"{remaining} piece(s) remaining";
        }
    }
}
