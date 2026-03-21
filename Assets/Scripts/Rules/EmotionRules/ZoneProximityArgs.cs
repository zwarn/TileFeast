using System;
using Pieces.Aspects;
using Zones;

namespace Rules.EmotionRules
{
    [Serializable]
    public class ZoneProximityArgs : EmotionRuleArgs
    {
        [UnityEngine.Tooltip("Leave null to apply to all pieces")]
        public AspectSO applyToAspect;

        [UnityEngine.Tooltip("The zone type to check (matched by reference)")]
        public ZoneSO targetZoneType;

        public ZoneProximityMode mode = ZoneProximityMode.OnZone;

        [UnityEngine.Tooltip("Emotion when the proximity condition is true")]
        public PieceEmotion emotionWhenTrue = PieceEmotion.Happy;

        [UnityEngine.Tooltip("Emotion when the proximity condition is false. Neutral returns null (no effect).")]
        public PieceEmotion emotionWhenFalse = PieceEmotion.Neutral;
    }
}
