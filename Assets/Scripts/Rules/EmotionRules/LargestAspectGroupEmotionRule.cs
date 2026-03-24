using System;
using System.Linq;
using Pieces;
using Pieces.Aspects;

namespace Rules.EmotionRules
{
    [Serializable]
    public class LargestAspectGroupEmotionRule : EmotionRule
    {
        [UnityEngine.Tooltip("Leave null to apply to all pieces")]
        public AspectSO applyToAspect;

        [UnityEngine.Tooltip("The aspect that defines the connected group")]
        public AspectSO groupAspect;

        [UnityEngine.Tooltip("Emotion when the piece is part of the largest group")]
        public PieceEmotion emotionWhenTrue = PieceEmotion.Happy;

        [UnityEngine.Tooltip("Emotion when the piece is not in the largest group. Neutral returns null (no effect).")]
        public PieceEmotion emotionWhenFalse = PieceEmotion.Neutral;

        public override EmotionEffect Evaluate(PlacedPiece piece, EmotionContext context)
        {
            if (applyToAspect != null && !piece.AllAspects.Contains(new Aspect(applyToAspect)))
                return null;

            var aspect = new Aspect(groupAspect);

            if (!piece.AllAspects.Contains(aspect))
            {
                if (emotionWhenFalse == PieceEmotion.Neutral) return null;
                return new EmotionEffect(emotionWhenFalse, $"Does not have the {groupAspect.name} aspect", this);
            }

            var groups = RulesHelper.GetGroups(context.TileArray,
                p => p != null && p.AllAspects.Contains(aspect));

            if (groups.Count == 0)
            {
                if (emotionWhenFalse == PieceEmotion.Neutral) return null;
                return new EmotionEffect(emotionWhenFalse, $"No {groupAspect.name} groups found", this);
            }

            int maxSize = groups.Max(g => g.Count);
            var pieceTiles = piece.GetTilePosition();

            bool inLargestGroup = groups
                .Where(g => g.Count == maxSize)
                .Any(g => pieceTiles.Any(t => g.Contains(t)));

            if (inLargestGroup)
                return new EmotionEffect(emotionWhenTrue,
                    $"Part of the largest {groupAspect.name} group (size {maxSize})", this);

            if (emotionWhenFalse == PieceEmotion.Neutral)
                return null;

            return new EmotionEffect(emotionWhenFalse,
                $"Not part of the largest {groupAspect.name} group (largest is {maxSize})", this);
        }

        public override string GetDescription()
        {
            var target = applyToAspect != null ? $"{applyToAspect.name} pieces" : "Pieces";
            return $"{target} are {emotionWhenTrue} when part of the largest {groupAspect.name} group";
        }
    }
}
