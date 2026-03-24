using System;
using System.Linq;
using Pieces;
using Pieces.Aspects;
using UnityEngine;

namespace Rules.EmotionRules
{
    [Serializable]
    public class EdgeProximityEmotionRule : EmotionRule
    {
        [UnityEngine.Tooltip("Leave null to apply to all pieces")]
        public AspectSO applyToAspect;

        [UnityEngine.Tooltip("Emotion when the piece is next to the edge of the board or a hole")]
        public PieceEmotion emotionWhenTrue = PieceEmotion.Happy;

        [UnityEngine.Tooltip("Emotion when the piece is not next to any edge or hole. Neutral returns null (no effect).")]
        public PieceEmotion emotionWhenFalse = PieceEmotion.Neutral;

        public override EmotionEffect Evaluate(PlacedPiece piece, EmotionContext context)
        {
            if (applyToAspect != null && !piece.AllAspects.Contains(new Aspect(applyToAspect)))
                return null;

            var width = context.TileArray.GetLength(0);
            var height = context.TileArray.GetLength(1);
            var blocked = context.State.BlockedPositions;

            bool nextToEdgeOrHole = RulesHelper.GetRawNeighborPositions(piece).Any(pos =>
                pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height ||
                blocked.Contains(pos));

            if (nextToEdgeOrHole)
                return new EmotionEffect(emotionWhenTrue, "Next to the edge or a hole", this);

            if (emotionWhenFalse == PieceEmotion.Neutral)
                return null;

            return new EmotionEffect(emotionWhenFalse, "Not next to any edge or hole", this);
        }

        public override string GetDescription()
        {
            var target = applyToAspect != null ? $"{applyToAspect.name} pieces" : "Pieces";
            return $"{target} are {emotionWhenTrue} when next to the edge or a hole";
        }
    }
}
