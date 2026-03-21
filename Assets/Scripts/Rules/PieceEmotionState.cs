using System.Collections.Generic;
using System.Linq;
using Pieces;

namespace Rules
{
    public class PieceEmotionState
    {
        public PlacedPiece Piece { get; }
        public List<EmotionEffect> Effects { get; }

        public PieceEmotionState(PlacedPiece piece, List<EmotionEffect> effects)
        {
            Piece = piece;
            Effects = effects;
        }

        // Most negative wins: Sad (2) > Neutral (1) > Happy (0)
        public PieceEmotion FinalEmotion =>
            Effects.Count == 0
                ? PieceEmotion.Neutral
                : (PieceEmotion)Effects.Max(e => (int)e.Emotion);
    }
}
