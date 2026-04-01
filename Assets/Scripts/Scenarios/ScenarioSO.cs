using System.Collections.Generic;
using System.Linq;
using Zones;
using Pieces;
using Rules.AspectSources;
using Rules.EmotionRules;
using Rules.CompletionRules;
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
        public List<Vector2Int> horizontalWalls = new();
        public List<Vector2Int> verticalWalls = new();
        public List<EmotionRuleConfig> emotionRules;
        public List<CompletionRuleConfig> completionRules;
        public List<Zone> zones;
        [SerializeField] private List<AspectSourceConfig> aspectSources;
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

        public List<AspectSourceConfig> AspectSources()
        {
            return aspectSources?.ToList() ?? new List<AspectSourceConfig>();
        }
    }
}
