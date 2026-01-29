using System.Collections.Generic;
using System.Linq;
using Board.Zone;
using Piece;
using Rules.PlacementRules;
using Rules.ScoreRules;
using Scenario;
using UnityEngine;

namespace Core
{
    public class GameState
    {
        public Vector2Int GridSize;
        public List<Piece.Piece> AvailablePieces;
        public List<Vector2Int> BlockedPositions;
        public PieceWithRotation PieceInHand;
        public List<PlacedPiece> PlacedPieces;
        public List<ScoreRuleSO> ScoreRules;
        public List<PlacementRuleSO> PlacementRules;
        public List<Zone> Zones;

        public GameState(Vector2Int gridSize, List<Vector2Int> blockedPositions, List<PlacedPiece> placedPieces, List<Piece.Piece> availablePieces,
            PieceWithRotation pieceInHand, List<ScoreRuleSO> scoreRules, List<PlacementRuleSO> placementRules, List<Zone> zones)
        {
            GridSize = gridSize;
            BlockedPositions = blockedPositions;
            PlacedPieces = placedPieces;
            AvailablePieces = availablePieces;
            PieceInHand = pieceInHand;
            ScoreRules = scoreRules;
            PlacementRules = placementRules;
            Zones = zones;
        }

        public GameState(ScenarioSO scenarioSO) :
            this(scenarioSO.gridSize,
                scenarioSO.blockedPositions.ToList(),
                scenarioSO.LockedPieces(),
                scenarioSO.AvailablePieces(),
                null,
                scenarioSO.scoreRules.ToList(),
                scenarioSO.placementRules.ToList(),
                scenarioSO.Zones())
        {
        }
    }
}