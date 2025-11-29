using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Rules.ZoneRules
{
    public class ZoneContext
    {
        public List<Vector2Int> ZonePosition { get; }
        public GameState State { get; }
        public Piece.Piece[,] TileArray { get; }

        public ZoneContext(List<Vector2Int> zonePosition, GameState gameState, Piece.Piece[,] tileArray)
        {
            ZonePosition = zonePosition;
            State = gameState;
            TileArray = tileArray;
        }
    }
}