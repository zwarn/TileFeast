using System;
using UnityEngine;

namespace Scenario
{
    public class ScenarioController : MonoBehaviour
    {
        [SerializeField] private ScenarioSO scenario;

        public Action<ScenarioSO> OnScenarioChanged;

        private void Start()
        {
            LoadScenario(scenario);
        }

        public void LoadScenario(ScenarioSO incomingScenario)
        {
            scenario = incomingScenario;
            ScenarioChangedEvent(scenario);
        }

        public void ScenarioChangedEvent(ScenarioSO incomingScenario)
        {
            OnScenarioChanged?.Invoke(incomingScenario);
        }
    }
}