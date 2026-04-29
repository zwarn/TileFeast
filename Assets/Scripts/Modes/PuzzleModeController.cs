using System;
using Rules;
using Scenarios;
using UnityEngine;
using Zenject;

namespace Modes
{
    public class PuzzleModeController : MonoBehaviour
    {
        [Inject] private ScenarioController _scenarioController;
        [Inject] private RulesController _rulesController;

        public event Action OnLevelComplete;

        private bool _active;

        private void OnEnable()
        {
            _rulesController.OnEvaluationChanged += HandleEvaluationChanged;
        }

        private void OnDisable()
        {
            _rulesController.OnEvaluationChanged -= HandleEvaluationChanged;
        }

        public void StartPuzzle(ScenarioSO scenario)
        {
            _active = true;
            _scenarioController.LoadScenario(scenario);
        }

        public void AdvanceToNextLevel()
        {
            _scenarioController.LoadNextScenario();
        }

        private void HandleEvaluationChanged(EmotionEvaluationResult _)
        {
            if (!_active) return;
            if (_rulesController.IsLevelComplete())
                OnLevelComplete?.Invoke();
        }
    }
}
