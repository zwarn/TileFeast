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

        public EmotionContext(GameState state, PlacedPiece[,] tileArray, List<Zone> zones)
        {
            State = state;
            TileArray = tileArray;
            Zones = zones;
        }
    }
}
