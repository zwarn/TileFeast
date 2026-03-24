using System;
using System.Linq;
using Pieces;
using Pieces.Aspects;

namespace Rules.EmotionRules
{
    /// <summary>
    /// Counts cardinal neighbor positions that are in-bounds, not blocked, and unoccupied.
    /// Edges and holes do NOT count as empty neighbors.
    /// </summary>
    [Serializable]
    public class EmptyNeighborCountEmotionRule : EmotionRule
    {
        [UnityEngine.Tooltip("Leave null to apply to all pieces")]
        public AspectSO applyToAspect;

        [UnityEngine.Tooltip("Minimum number of empty neighbor positions (inclusive). Edges and holes are excluded.")]
        public int minCount = 1;

        [UnityEngine.Tooltip("Maximum number of empty neighbor positions (-1 = unlimited)")]
        public int maxCount = -1;

        [UnityEngine.Tooltip("Emotion when the condition is met")]
        public PieceEmotion emotionWhenMet = PieceEmotion.Happy;

        [UnityEngine.Tooltip("Emotion when the condition is not met. Neutral returns null (no effect).")]
        public PieceEmotion emotionWhenNotMet = PieceEmotion.Neutral;

        public override EmotionEffect Evaluate(PlacedPiece piece, EmotionContext context)
        {
            if (applyToAspect != null && !piece.AllAspects.Contains(new Aspect(applyToAspect)))
                return null;

            var blocked = context.State.BlockedPositions;
            var tileArray = context.TileArray;

            int count = RulesHelper.GetNeighborPositions(piece, tileArray)
                .Count(pos => !blocked.Contains(pos) && tileArray[pos.x, pos.y] == null);

            bool conditionMet = count >= minCount && (maxCount < 0 || count <= maxCount);

            if (conditionMet)
                return new EmotionEffect(emotionWhenMet, $"Has {count} empty neighbor(s)", this);

            if (emotionWhenNotMet == PieceEmotion.Neutral)
                return null;

            return new EmotionEffect(emotionWhenNotMet,
                $"Only {count} empty neighbor(s) (needs {minCount})", this);
        }

        public override string GetDescription()
        {
            var target = applyToAspect != null ? $"{applyToAspect.name} pieces" : "Pieces";
            var range = maxCount >= 0 ? $"{minCount}-{maxCount}" : $"{minCount}+";
            return $"{target} are {emotionWhenMet} when having {range} empty neighbor(s)";
        }
    }
}
