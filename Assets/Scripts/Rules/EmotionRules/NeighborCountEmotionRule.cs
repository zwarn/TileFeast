using System;
using System.Linq;
using Pieces;
using Pieces.Aspects;

namespace Rules.EmotionRules
{
    [Serializable]
    public class NeighborCountEmotionRule : EmotionRule
    {
        [UnityEngine.Tooltip("Leave null to apply to all pieces")]
        public AspectSO applyToAspect;

        [UnityEngine.Tooltip("Minimum number of neighboring pieces to trigger the condition (inclusive). Set both min and max to 0 for 'isolated'.")]
        public int minCount = 1;

        [UnityEngine.Tooltip("Maximum number of neighboring pieces to trigger the condition (-1 = unlimited)")]
        public int maxCount = -1;

        [UnityEngine.Tooltip("Emotion when the condition is met")]
        public PieceEmotion emotionWhenMet = PieceEmotion.Happy;

        [UnityEngine.Tooltip("Emotion when the condition is not met. Neutral returns null (no effect).")]
        public PieceEmotion emotionWhenNotMet = PieceEmotion.Neutral;

        public override EmotionEffect Evaluate(PlacedPiece piece, EmotionContext context)
        {
            if (applyToAspect != null && !piece.AllAspects.Contains(new Aspect(applyToAspect)))
                return null;

            int count = RulesHelper.GetNeighborPieces(piece, context.TileArray).Count;

            bool conditionMet = count >= minCount && (maxCount < 0 || count <= maxCount);

            if (conditionMet)
            {
                var reason = maxCount == 0 ? "Isolated (no neighbors)" : $"Has {count} neighbor(s)";
                return new EmotionEffect(emotionWhenMet, reason, this);
            }

            if (emotionWhenNotMet == PieceEmotion.Neutral)
                return null;

            return new EmotionEffect(emotionWhenNotMet,
                $"Has {count} neighbor(s) (needs {minCount}+)", this);
        }

        public override string GetDescription()
        {
            var target = applyToAspect != null ? $"{applyToAspect.name} pieces" : "Pieces";
            if (minCount == 0 && maxCount == 0)
                return $"{target} are {emotionWhenMet} when isolated (no neighbors)";
            var range = maxCount >= 0 ? $"{minCount}-{maxCount}" : $"{minCount}+";
            return $"{target} are {emotionWhenMet} when having {range} neighbor(s)";
        }
    }
}
