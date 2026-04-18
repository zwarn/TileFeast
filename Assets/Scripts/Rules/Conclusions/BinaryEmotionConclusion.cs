using System;
using Rules.Checks;
using Rules.EmotionRules;

namespace Rules.Conclusions
{
    /// <summary>
    /// Emits <see cref="whenPassed"/> when the check passes, and <see cref="whenFailed"/>
    /// otherwise. If the chosen emotion is Neutral the effect is dropped (null), matching
    /// the previous subclass-per-rule behaviour.
    /// </summary>
    [Serializable]
    public class BinaryEmotionConclusion : EmotionConclusion
    {
        [UnityEngine.Tooltip("Emotion applied when the check passes. Neutral means no effect is emitted.")]
        public PieceEmotion whenPassed = PieceEmotion.Happy;

        [UnityEngine.Tooltip("Emotion applied when the check fails. Neutral means no effect is emitted.")]
        public PieceEmotion whenFailed = PieceEmotion.Neutral;

        public override EmotionEffect Build(CheckResult result, EmotionRule source, EmotionCheck check)
        {
            if (result == null) return null;
            var emotion = result.Passed ? whenPassed : whenFailed;
            if (emotion == PieceEmotion.Neutral) return null;

            string reason = BuildReason(result, check);
            return new EmotionEffect(emotion, reason, source);
        }

        public override string GetDescription()
        {
            if (whenFailed == PieceEmotion.Neutral) return whenPassed.ToString();
            return $"{whenPassed}, else {whenFailed}";
        }

        private static string BuildReason(CheckResult result, EmotionCheck check)
        {
            string detail = result.DetailReason;
            if (!string.IsNullOrEmpty(detail)) return detail;
            return check != null ? check.GetDescription() : string.Empty;
        }
    }
}
