using System.Collections.Generic;
using Piece;
using Rules.Placement;
using Rules.Score;
using UnityEngine;

namespace Scenario
{
    [CreateAssetMenu(fileName = "Scenario", menuName = "Scenario", order = 0)]
    public class ScenarioSO : ScriptableObject
    {
        public List<PieceSO> availablePieces;
        public List<ScoreRuleSO> scoreRules;
        public List<PlacementRuleSO> placementRules;
        public ScenarioSO nextLevel;
    }
}