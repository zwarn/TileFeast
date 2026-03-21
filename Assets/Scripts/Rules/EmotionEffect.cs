using Rules.EmotionRules;

namespace Rules
{
    public class EmotionEffect
    {
        public PieceEmotion Emotion { get; }
        public string Reason { get; }
        public EmotionRuleSO Source { get; }

        public EmotionEffect(PieceEmotion emotion, string reason, EmotionRuleSO source)
        {
            Emotion = emotion;
            Reason = reason;
            Source = source;
        }
    }
}
