using System;
using System.Linq;
using Pieces;

namespace Rules.Filters
{
    /// <summary>
    /// Matches pieces whose final emotion from the previous evaluation tick equals the target.
    /// On the very first tick (no previous result) treats every piece as Neutral.
    /// </summary>
    [Serializable]
    public class PreviousEmotionFilter : PieceFilter
    {
        [UnityEngine.Tooltip("Emotion the piece had on the previous evaluation tick")]
        public PieceEmotion emotion = PieceEmotion.Happy;

        public override bool Matches(PlacedPiece piece, EmotionContext context)
        {
            var previous = GetPreviousEmotion(piece, context);
            return previous == emotion;
        }

        public override string GetDescription() => $"pieces that were {emotion}";

        private static PieceEmotion GetPreviousEmotion(PlacedPiece piece, EmotionContext context)
        {
            if (context.PreviousResult == null) return PieceEmotion.Neutral;
            var state = context.PreviousResult.PieceStates.FirstOrDefault(s => s.Piece == piece);
            return state?.FinalEmotion ?? PieceEmotion.Neutral;
        }
    }
}
