using Rules.EmotionRules;

namespace Rules
{
    public class EmotionEffect
    {
        public PieceEmotion Emotion { get; }
        public string Reason { get; }
        public EmotionRule Source { get; }

        public EmotionEffect(PieceEmotion emotion, string reason, EmotionRule source)
        {
            Emotion = emotion;
            Reason = reason;
            Source = source;
        }
    }
}
