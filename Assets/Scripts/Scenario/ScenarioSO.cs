using System.Collections.Generic;
using System.Linq;
using Board.Zone;
using Piece;
using Rules.Placement;
using Rules.Score;
using UnityEngine;

namespace Scenario
{
    [CreateAssetMenu(fileName = "Scenario", menuName = "Scenario", order = 0)]
    public class ScenarioSO : ScriptableObject
    {
        public Vector2Int gridSize = new(9, 9);
        [SerializeField] private List<PieceSO> availablePieces;
        [SerializeField] private LockedPieceList lockedPieces;
        public List<Vector2Int> blockedPositions;
        public List<ScoreRuleSO> scoreRules;
        public List<PlacementRuleSO> placementRules;
        public List<Zone> zones;
        public ScenarioSO nextLevel;

        public List<Piece.Piece> AvailablePieces()
        {
            return availablePieces.Select(so => new Piece.Piece(so, false)).ToList();
        }

        public List<PlacedPiece> LockedPieces()
        {
            return lockedPieces.LockedPieces();
        }
    }
}