using System.Collections.Generic;
using Piece.model;
using Score;
using UnityEngine;

namespace Scenario
{
    [CreateAssetMenu(fileName = "Scenario", menuName = "Scenario", order = 0)]
    public class ScenarioSO : ScriptableObject
    {
        public List<PieceSO> availablePieces;
        public List<ScoreRule> scoreRules;
        public ScenarioSO nextLevel;
    }
}