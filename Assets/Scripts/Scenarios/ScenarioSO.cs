using System.Collections.Generic;
using System.Linq;
using Zones;
using Pieces;
using Rules.PlacementRules;
using Rules.ScoreRules;
using UnityEngine;

namespace Scenarios
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

        public List<Piece> AvailablePieces()
        {
            return availablePieces.Select(so => new Piece(so, false)).ToList();
        }

        public List<PlacedPiece> LockedPieces()
        {
            return lockedPieces.LockedPieces();
        }

        public List<Zone> Zones()
        {
            return zones.Select(zone => zone.Clone()).ToList();
        }
    }
}