using System;
using System.Collections.Generic;
using System.Linq;
using Zones;
using Pieces;
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
        public PieceWithRotation PieceInHand;
        public List<PlacedPiece> PlacedPieces;
        public List<EmotionRuleConfig> EmotionRules;
        public List<CompletionRuleConfig> CompletionRules;
        public List<Zone> Zones;

        public GameState(Vector2Int gridSize, List<Vector2Int> blockedPositions, List<PlacedPiece> placedPieces,
            List<Piece> availablePieces, PieceWithRotation pieceInHand,
            List<EmotionRuleConfig> emotionRules, List<CompletionRuleConfig> completionRules, List<Zone> zones)
        {
            GridSize = gridSize;
            BlockedPositions = blockedPositions;
            PlacedPieces = placedPieces;
            AvailablePieces = availablePieces;
            PieceInHand = pieceInHand;
            EmotionRules = emotionRules;
            CompletionRules = completionRules;
            Zones = zones;
        }

        public GameState(ScenarioSO scenarioSO) :
            this(scenarioSO.gridSize,
                scenarioSO.blockedPositions.ToList(),
                scenarioSO.LockedPieces(),
                scenarioSO.AvailablePieces(),
                null,
                scenarioSO.emotionRules.ToList(),
                scenarioSO.completionRules.ToList(),
                scenarioSO.Zones())
        {
        }
    }
}
