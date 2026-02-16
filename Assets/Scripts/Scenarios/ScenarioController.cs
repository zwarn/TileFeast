using Core;
using UnityEngine;
using Zenject;

namespace Scenarios
{
    public class ScenarioController : MonoBehaviour
    {
        [SerializeField] private ScenarioSO scenario;

        [Inject] private GameController _gameController;

        private void Start()
        {
            LoadScenario(scenario);
        }

        public void LoadScenario(ScenarioSO incomingScenario)
        {
            scenario = incomingScenario;
            _gameController.LoadScenario(scenario);
        }

        public void LoadNextScenario()
        {
            if (scenario.nextLevel == null) return;

            LoadScenario(scenario.nextLevel);
        }
    }
}