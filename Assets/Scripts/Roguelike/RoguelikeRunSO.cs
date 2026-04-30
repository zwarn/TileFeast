using System.Collections.Generic;
using Roguelike.Generators;
using Scenarios;
using UnityEngine;

namespace Roguelike
{
    [CreateAssetMenu(fileName = "RoguelikeRun", menuName = "Roguelike/Roguelike Run")]
    public class RoguelikeRunSO : ScriptableObject
    {
        public ScenarioSO startingScenario;
        public int startingHealth = 10;

        [Header("Turn Schedule")]
        [Tooltip("Repeating pattern — turn N uses index (N % pattern.Count)")]
        public List<OfferGroupConfig> turnPattern;

        [Header("Generators")]
        public PieceOfferGeneratorSO pieceGenerator;
        public PlaceableOfferGeneratorSO placeableGenerator;
        public RuleOfferGeneratorSO ruleGenerator;
    }
}
