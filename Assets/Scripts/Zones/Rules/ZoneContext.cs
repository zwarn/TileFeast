using System.Collections.Generic;
using Core;
using Pieces;
using UnityEngine;

namespace Zones.Rules
{
    public class ZoneContext
    {
        public List<Vector2Int> ZonePosition { get; }
        public GameState State { get; }
        public Piece[,] TileArray { get; }

        public ZoneContext(List<Vector2Int> zonePosition, GameState gameState, Piece[,] tileArray)
        {
            ZonePosition = zonePosition;
            State = gameState;
            TileArray = tileArray;
        }
    }
}