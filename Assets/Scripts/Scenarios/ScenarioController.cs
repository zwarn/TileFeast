using Core;
using UnityEngine;
using Zenject;

namespace Scenarios
{
    public class ScenarioController : MonoBehaviour
    {
        [SerializeField] private ScenarioSO scenario;
        [SerializeField] private bool autoLoadOnStart = true;

        [Inject] private GameController _gameController;

        public ScenarioSO CurrentScenario => scenario;

        private void Start()
        {
            if (autoLoadOnStart && scenario != null)
                LoadScenario(scenario);
        }

        public void LoadScenario(ScenarioSO incomingScenario, bool addProceduralItems = true)
        {
            scenario = incomingScenario;
            _gameController.LoadScenario(scenario, addProceduralItems);
        }

        public void LoadNextScenario()
        {
            if (scenario.nextLevel == null) return;

            LoadScenario(scenario.nextLevel);
        }
    }
}