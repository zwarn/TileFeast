using System.Collections.Generic;
using System.Linq;

namespace Rules
{
    public class EmotionEvaluationResult
    {
        public IReadOnlyList<PieceEmotionState> PieceStates { get; }

        public int HappyCount => PieceStates.Count(s => s.FinalEmotion == PieceEmotion.Happy);
        public int NeutralCount => PieceStates.Count(s => s.FinalEmotion == PieceEmotion.Neutral);
        public int SadCount => PieceStates.Count(s => s.FinalEmotion == PieceEmotion.Sad);

        // Score = number of happy pieces
        public int Score => HappyCount;

        public EmotionEvaluationResult(List<PieceEmotionState> pieceStates)
        {
            PieceStates = pieceStates;
        }

        public static EmotionEvaluationResult Empty() =>
            new EmotionEvaluationResult(new List<PieceEmotionState>());
    }
}
