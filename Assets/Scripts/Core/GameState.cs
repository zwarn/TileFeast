using System;
using System.Collections.Generic;
using System.Linq;
using Zones;
using Pieces;
using Rules.AspectSources;
using Rules.EmotionRules;
using Rules.CompletionRules;
using Scenarios;
using UnityEngine;

namespace Core
{
    [Serializable]
    public class GameState
    {
        public Vector2Int GridSize;
        public List<Piece> AvailablePieces;
        public List<Vector2Int> BlockedPositions;
        public List<Vector2Int> HorizontalWalls;
        public List<Vector2Int> VerticalWalls;
        public PieceWithRotation PieceInHand;
        public List<PlacedPiece> PlacedPieces;
        public List<EmotionRuleConfig> EmotionRules;
        public List<CompletionRuleConfig> CompletionRules;
        public List<Zone> Zones;
        public List<AspectSourceConfig> AspectSources;

        public GameState(Vector2Int gridSize, List<Vector2Int> blockedPositions, List<PlacedPiece> placedPieces,
            List<Piece> availablePieces, PieceWithRotation pieceInHand,
            List<EmotionRuleConfig> emotionRules, List<CompletionRuleConfig> completionRules, List<Zone> zones,
            List<AspectSourceConfig> aspectSources = null,
            List<Vector2Int> horizontalWalls = null,
            List<Vector2Int> verticalWalls = null)
        {
            GridSize = gridSize;
            BlockedPositions = blockedPositions;
            HorizontalWalls = horizontalWalls ?? new List<Vector2Int>();
            VerticalWalls = verticalWalls ?? new List<Vector2Int>();
            PlacedPieces = placedPieces;
            AvailablePieces = availablePieces;
            PieceInHand = pieceInHand;
            EmotionRules = emotionRules;
            CompletionRules = completionRules;
            Zones = zones;
            AspectSources = aspectSources ?? new List<AspectSourceConfig>();
        }

        public GameState(ScenarioSO scenarioSO) :
            this(scenarioSO.gridSize,
                scenarioSO.blockedPositions.ToList(),
                scenarioSO.LockedPieces(),
                scenarioSO.AvailablePieces(),
                null,
                scenarioSO.emotionRules.ToList(),
                scenarioSO.completionRules.ToList(),
                scenarioSO.Zones(),
                scenarioSO.AspectSources(),
                scenarioSO.horizontalWalls?.ToList() ?? new List<Vector2Int>(),
                scenarioSO.verticalWalls?.ToList() ?? new List<Vector2Int>())
        {
        }
    }
}
