using System;
using System.Linq;
using Pieces;
using Pieces.Aspects;

namespace Rules.EmotionRules
{
    [Serializable]
    public class AspectGroupSizeEmotionRule : EmotionRule
    {
        [UnityEngine.Tooltip("Leave null to apply to all pieces")]
        public AspectSO applyToAspect;

        [UnityEngine.Tooltip("The aspect that defines the connected group")]
        public AspectSO groupAspect;

        [UnityEngine.Tooltip("Minimum group size to trigger the condition (inclusive)")]
        public int minSize = 1;

        [UnityEngine.Tooltip("Maximum group size to trigger the condition (-1 = unlimited)")]
        public int maxSize = -1;

        [UnityEngine.Tooltip("Emotion when the group size condition is met")]
        public PieceEmotion emotionWhenMet = PieceEmotion.Happy;

        [UnityEngine.Tooltip("Emotion when the group size condition is not met. Neutral returns null (no effect).")]
        public PieceEmotion emotionWhenNotMet = PieceEmotion.Neutral;

        public override EmotionEffect Evaluate(PlacedPiece piece, EmotionContext context)
        {
            if (applyToAspect != null && !piece.AllAspects.Contains(new Aspect(applyToAspect)))
                return null;

            var aspect = new Aspect(groupAspect);

            if (!piece.AllAspects.Contains(aspect))
            {
                if (emotionWhenNotMet == PieceEmotion.Neutral) return null;
                return new EmotionEffect(emotionWhenNotMet, $"Does not have the {groupAspect.name} aspect", this);
            }

            var groups = RulesHelper.GetGroups(context.TileArray,
                p => p != null && p.AllAspects.Contains(aspect));

            var pieceTiles = piece.GetTilePosition();
            var myGroup = groups.FirstOrDefault(g => pieceTiles.Any(t => g.Contains(t)));
            int groupSize = myGroup?.Count ?? 0;

            bool conditionMet = groupSize >= minSize && (maxSize < 0 || groupSize <= maxSize);

            if (conditionMet)
                return new EmotionEffect(emotionWhenMet,
                    $"Part of a {groupAspect.name} group of size {groupSize}", this);

            if (emotionWhenNotMet == PieceEmotion.Neutral)
                return null;

            return new EmotionEffect(emotionWhenNotMet,
                $"{groupAspect.name} group size {groupSize} (needs {minSize}+)", this);
        }

        public override string GetDescription()
        {
            var target = applyToAspect != null ? $"{applyToAspect.name} pieces" : "Pieces";
            var range = maxSize >= 0 ? $"{minSize}-{maxSize}" : $"{minSize}+";
            return $"{target} are {emotionWhenMet} when part of a {groupAspect.name} group of size {range}";
        }
    }
}
