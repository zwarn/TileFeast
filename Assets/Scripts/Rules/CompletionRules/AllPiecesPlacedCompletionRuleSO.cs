using Core;
using UnityEngine;

namespace Rules.CompletionRules
{
    [CreateAssetMenu(menuName = "CompletionRule/AllPiecesPlaced", fileName = "AllPiecesPlacedCompletionRule")]
    public class AllPiecesPlacedCompletionRuleSO : CompletionRuleSO
    {
        public override bool IsMet(EmotionEvaluationResult result, GameState state, CompletionRuleArgs args)
            => state.PieceInHand == null && state.AvailablePieces.Count == 0;

        public override string GetDescription(CompletionRuleArgs args)
            => "All pieces must be placed on the board";

        public override string GetProgress(EmotionEvaluationResult result, GameState state, CompletionRuleArgs args)
        {
            int remaining = state.AvailablePieces.Count + (state.PieceInHand != null ? 1 : 0);
            return remaining == 0 ? "All placed" : $"{remaining} piece(s) remaining";
        }
    }
}
