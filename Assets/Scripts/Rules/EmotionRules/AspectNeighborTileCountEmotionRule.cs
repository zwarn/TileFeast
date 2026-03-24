using System;
using System.Linq;
using Pieces;
using Pieces.Aspects;

namespace Rules.EmotionRules
{
    /// <summary>
    /// Counts how many neighboring TILE POSITIONS are covered by a piece with the given aspect.
    /// Unlike AspectAdjacencyEmotionRule (which counts distinct neighbor pieces), this counts tile positions —
    /// so a large multi-tile neighbor piece can contribute multiple tiles to the count.
    /// </summary>
    [Serializable]
    public class AspectNeighborTileCountEmotionRule : EmotionRule
    {
        [UnityEngine.Tooltip("Leave null to apply to all pieces")]
        public AspectSO applyToAspect;

        [UnityEngine.Tooltip("The aspect to look for on neighboring tile positions")]
        public AspectSO neighborAspect;

        [UnityEngine.Tooltip("Minimum number of neighboring tiles covered by the aspect (inclusive)")]
        public int minCount = 1;

        [UnityEngine.Tooltip("Maximum number of neighboring tiles covered by the aspect (-1 = unlimited)")]
        public int maxCount = -1;

        [UnityEngine.Tooltip("Emotion when the condition is met")]
        public PieceEmotion emotionWhenMet = PieceEmotion.Happy;

        [UnityEngine.Tooltip("Emotion when the condition is not met. Neutral returns null (no effect).")]
        public PieceEmotion emotionWhenNotMet = PieceEmotion.Neutral;

        public override EmotionEffect Evaluate(PlacedPiece piece, EmotionContext context)
        {
            if (applyToAspect != null && !piece.AllAspects.Contains(new Aspect(applyToAspect)))
                return null;

            var aspect = new Aspect(neighborAspect);
            var tileArray = context.TileArray;

            int count = RulesHelper.GetNeighborPositions(piece, tileArray)
                .Count(pos => tileArray[pos.x, pos.y] != null &&
                              tileArray[pos.x, pos.y].AllAspects.Contains(aspect));

            bool conditionMet = count >= minCount && (maxCount < 0 || count <= maxCount);

            if (conditionMet)
                return new EmotionEffect(emotionWhenMet,
                    $"{count} neighboring tile(s) covered by {neighborAspect.name}", this);

            if (emotionWhenNotMet == PieceEmotion.Neutral)
                return null;

            return new EmotionEffect(emotionWhenNotMet,
                $"Only {count} neighboring tile(s) covered by {neighborAspect.name} (needs {minCount})", this);
        }

        public override string GetDescription()
        {
            var target = applyToAspect != null ? $"{applyToAspect.name} pieces" : "Pieces";
            var range = maxCount >= 0 ? $"{minCount}-{maxCount}" : $"{minCount}+";
            return $"{target} are {emotionWhenMet} when {range} neighboring tile(s) are covered by {neighborAspect.name}";
        }
    }
}
