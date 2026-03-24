using System.Collections.Generic;
using Core;
using Pieces;
using Zones;

namespace Rules
{
    public class EmotionContext
    {
        public GameState State { get; }
        public PlacedPiece[,] TileArray { get; }
        public List<Zone> Zones { get; }
        /// <summary>Emotion states from the previous evaluation tick. Null on the very first evaluation.</summary>
        public EmotionEvaluationResult PreviousResult { get; }

        public EmotionContext(GameState state, PlacedPiece[,] tileArray, List<Zone> zones,
            EmotionEvaluationResult previousResult = null)
        {
            State = state;
            TileArray = tileArray;
            Zones = zones;
            PreviousResult = previousResult;
        }
    }
}
