using System.Collections.Generic;
using Scenarios;
using UnityEngine;

namespace Roguelike
{
    [CreateAssetMenu(fileName = "RoguelikeRun", menuName = "Roguelike/Roguelike Run")]
    public class RoguelikeRunSO : ScriptableObject
    {
        public ScenarioSO startingScenario;
        public int startingHealth = 10;
        public int draftGroupsPerTurn = 1;
        public List<RoguelikeOfferSO> offerPool = new();
    }
}
