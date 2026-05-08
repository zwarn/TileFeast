using Core;
using Pieces;
using Rules;
using Rules.CompletionRules;
using Rules.EmotionRules;

namespace UI.Tooltip
{
    public abstract class TooltipData { }

    public class PieceTooltipData : TooltipData
    {
        public PlacedPiece Piece;
        public PieceEmotionState EmotionState; // null = no emotion rules matched this piece
    }

    public class EmotionRuleTooltipData : TooltipData
    {
        public EmotionRule Rule;
        public EmotionEvaluationResult EvaluationResult;
    }

    public class CompletionRuleTooltipData : TooltipData
    {
        public CompletionRuleConfig Config;
        public EmotionEvaluationResult EvaluationResult;
        public GameState State;
    }
}
