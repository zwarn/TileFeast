using System;
using State;
using UnityEngine;
using Zenject;

namespace Scenario
{
    public class ScenarioController : MonoBehaviour
    {
        [SerializeField] private ScenarioSO scenario;

        [Inject] private GameStateController _gameStateController;

        private void Start()
        {
            LoadScenario(scenario);
        }

        public void LoadScenario(ScenarioSO incomingScenario)
        {
            scenario = incomingScenario;
            _gameStateController.LoadScenario(scenario);
        }
    }
}