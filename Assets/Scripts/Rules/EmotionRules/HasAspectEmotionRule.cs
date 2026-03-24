using System;
using System.Linq;
using Pieces;
using Pieces.Aspects;

namespace Rules.EmotionRules
{
    [Serializable]
    public class HasAspectEmotionRule : EmotionRule
    {
        [UnityEngine.Tooltip("Leave null to apply to all pieces")]
        public AspectSO applyToAspect;

        [UnityEngine.Tooltip("The aspect to check for on the piece itself")]
        public AspectSO targetAspect;

        [UnityEngine.Tooltip("Emotion when the piece has the target aspect")]
        public PieceEmotion emotionWhenTrue = PieceEmotion.Happy;

        [UnityEngine.Tooltip("Emotion when the piece does not have the target aspect. Neutral returns null (no effect).")]
        public PieceEmotion emotionWhenFalse = PieceEmotion.Neutral;

        public override EmotionEffect Evaluate(PlacedPiece piece, EmotionContext context)
        {
            if (applyToAspect != null && !piece.AllAspects.Contains(new Aspect(applyToAspect)))
                return null;

            bool hasAspect = piece.AllAspects.Contains(new Aspect(targetAspect));

            if (hasAspect)
                return new EmotionEffect(emotionWhenTrue, $"Has the {targetAspect.name} aspect", this);

            if (emotionWhenFalse == PieceEmotion.Neutral)
                return null;

            return new EmotionEffect(emotionWhenFalse, $"Missing the {targetAspect.name} aspect", this);
        }

        public override string GetDescription()
        {
            var target = applyToAspect != null ? $"{applyToAspect.name} pieces" : "Pieces";
            return $"{target} are {emotionWhenTrue} when having the {targetAspect.name} aspect";
        }
    }
}
