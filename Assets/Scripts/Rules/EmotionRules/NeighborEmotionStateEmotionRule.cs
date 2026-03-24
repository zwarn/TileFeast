using System;
using System.Linq;
using Pieces;
using Pieces.Aspects;

namespace Rules.EmotionRules
{
    /// <summary>
    /// Reacts to the emotion state of neighboring pieces from the PREVIOUS evaluation tick.
    /// Reading from the previous tick avoids evaluation cycles — oscillation across ticks is possible by design.
    /// </summary>
    [Serializable]
    public class NeighborEmotionStateEmotionRule : EmotionRule
    {
        [UnityEngine.Tooltip("Leave null to apply to all pieces")]
        public AspectSO applyToAspect;

        [UnityEngine.Tooltip("The emotion state to count on cardinal neighbors (from the previous evaluation)")]
        public PieceEmotion targetEmotion = PieceEmotion.Happy;

        [UnityEngine.Tooltip("Minimum number of neighbors with the target emotion to trigger the condition (inclusive)")]
        public int minCount = 1;

        [UnityEngine.Tooltip("Maximum number of neighbors with the target emotion (-1 = unlimited)")]
        public int maxCount = -1;

        [UnityEngine.Tooltip("Emotion when the condition is met")]
        public PieceEmotion emotionWhenMet = PieceEmotion.Happy;

        [UnityEngine.Tooltip("Emotion when the condition is not met. Neutral returns null (no effect).")]
        public PieceEmotion emotionWhenNotMet = PieceEmotion.Neutral;

        public override EmotionEffect Evaluate(PlacedPiece piece, EmotionContext context)
        {
            if (applyToAspect != null && !piece.AllAspects.Contains(new Aspect(applyToAspect)))
                return null;

            var neighbors = RulesHelper.GetNeighborPieces(piece, context.TileArray);

            int count = neighbors.Count(n => GetPreviousEmotion(n, context) == targetEmotion);

            bool conditionMet = count >= minCount && (maxCount < 0 || count <= maxCount);

            if (conditionMet)
                return new EmotionEffect(emotionWhenMet,
                    $"Adjacent to {count} {targetEmotion} neighbor(s)", this);

            if (emotionWhenNotMet == PieceEmotion.Neutral)
                return null;

            return new EmotionEffect(emotionWhenNotMet,
                $"Only {count} {targetEmotion} neighbor(s) (needs {minCount})", this);
        }

        private static PieceEmotion GetPreviousEmotion(PlacedPiece neighbor, EmotionContext context)
        {
            if (context.PreviousResult == null)
                return PieceEmotion.Neutral;

            var state = context.PreviousResult.PieceStates.FirstOrDefault(s => s.Piece == neighbor);
            return state?.FinalEmotion ?? PieceEmotion.Neutral;
        }

        public override string GetDescription()
        {
            var target = applyToAspect != null ? $"{applyToAspect.name} pieces" : "Pieces";
            var range = maxCount >= 0 ? $"{minCount}-{maxCount}" : $"{minCount}+";
            return $"{target} are {emotionWhenMet} when next to {range} {targetEmotion} neighbor(s)";
        }
    }
}
