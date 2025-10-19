using System.Collections.Generic;
using System.Linq;
using Piece;
using Rules.Placement;
using Rules.Score;
using UnityEngine;

namespace Scenario
{
    [CreateAssetMenu(fileName = "Scenario", menuName = "Scenario", order = 0)]
    public class ScenarioSO : ScriptableObject
    {
        public Vector2Int gridSize = new Vector2Int(9, 9);
        [SerializeField] private List<PieceSO> availablePieces;
        public List<ScoreRuleSO> scoreRules;
        public List<PlacementRuleSO> placementRules;
        public ScenarioSO nextLevel;

        public List<Piece.Piece> AvailablePieces()
        {
            return availablePieces.Select(so => new Piece.Piece(so)).ToList();
        }
    }
}