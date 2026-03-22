using System;
using Pieces.Aspects;

namespace Rules.EmotionRules
{
    [Serializable]
    public class AspectAdjacencyArgs : EmotionRuleArgs
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
    }
}