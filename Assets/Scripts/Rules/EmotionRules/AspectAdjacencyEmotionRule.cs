using System;
using System.Linq;
using Pieces;
using Pieces.Aspects;

namespace Rules.EmotionRules
{
    [Serializable]
    public class AspectAdjacencyEmotionRule : EmotionRule
    {
        [UnityEngine.Tooltip("Leave null to apply to all pieces")]
        public AspectSO applyToAspect;

        [UnityEngine.Tooltip("The aspect to count on cardinal neighbors")]
        public AspectSO neighborAspect;

        [UnityEngine.Tooltip("Minimum neighbor count to trigger the condition (inclusive)")]
        public int minNeighborCount = 1;

        [UnityEngine.Tooltip("Maximum neighbor count to trigger the condition (-1 = unlimited)")]
        public int maxNeighborCount = -1;

        [UnityEngine.Tooltip("Emotion when condition is met")]
        public PieceEmotion emotionWhenMet = PieceEmotion.Happy;

        [UnityEngine.Tooltip("Emotion when condition is not met. Neutral returns null (no effect).")]
        public PieceEmotion emotionWhenNotMet = PieceEmotion.Neutral;

        public override EmotionEffect Evaluate(PlacedPiece piece, EmotionContext context)
        {
            if (applyToAspect != null && !piece.AllAspects.Contains(new Aspect(applyToAspect)))
                return null;

            var neighbors = RulesHelper.GetNeighborPieces(piece, context.TileArray);
            int count = neighbors.Count(n => n.AllAspects.Contains(new Aspect(neighborAspect)));

            bool conditionMet = count >= minNeighborCount &&
                                (maxNeighborCount < 0 || count <= maxNeighborCount);

            if (conditionMet)
            {
                return new EmotionEffect(emotionWhenMet,
                    $"Adjacent to {count} {neighborAspect.name} neighbor(s)", this);
            }

            if (emotionWhenNotMet == PieceEmotion.Neutral)
                return null;

            return new EmotionEffect(emotionWhenNotMet,
                $"Only {count} {neighborAspect.name} neighbor(s) (needs {minNeighborCount})", this);
        }

        public override string GetDescription()
        {
            var target = applyToAspect != null ? $"{applyToAspect.name} pieces" : "Pieces";
            var range = maxNeighborCount >= 0
                ? $"{minNeighborCount}-{maxNeighborCount}"
                : $"{minNeighborCount}+";
            return $"{target} are {emotionWhenMet} when adjacent to {range} {neighborAspect.name} tile(s)";
        }
    }
}
