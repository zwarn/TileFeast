using Core;
using Rules;
using Rules.CompletionRules;
using Scenarios;
using TMPro;
using UI.Common;
using UnityEngine;
using Zenject;

namespace UI.Rules
{
    public class EmotionStatusPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text happinessConditionLabel;
        [SerializeField] private TMP_Text sadnessConditionLabel;
        [SerializeField] private Checkmark checkmark;

        [Inject] private RulesController _rulesController;
        [Inject] private ScenarioController _scenarioController;
        [Inject] private GameController _gameController;

        private void OnEnable()
        {
            _rulesController.OnEvaluationChanged += Refresh;
            Refresh(_rulesController.LastResult);
        }

        private void OnDisable()
        {
            _rulesController.OnEvaluationChanged -= Refresh;
        }

        private void Refresh(EmotionEvaluationResult result)
        {
            var rules = _gameController.CurrentState?.CompletionRules;

            MinHappyCountCompletionRule happyRule = null;
            MaxSadCountCompletionRule sadRule = null;

            if (rules != null)
            {
                foreach (var config in rules)
                {
                    if (config.rule is MinHappyCountCompletionRule h) happyRule = h;
                    else if (config.rule is MaxSadCountCompletionRule s) sadRule = s;
                }
            }

            string happyLine = happyRule != null
                ? $"Happy: {result.HappyCount}/{happyRule.minimumHappyPieces}"
                : $"Happy: {result.HappyCount}";

            happinessConditionLabel.text = happyLine;

            string sadLine = (sadRule != null)
                ? $"Sad: {result.SadCount}/{sadRule.maximumSadPieces}"
                : $"Sad: {result.SadCount}";

            sadnessConditionLabel.text = sadLine;

            bool complete = _rulesController.IsLevelComplete();
            if (checkmark != null) checkmark.SetState(complete);
        }

        public void GoToNextScenario()
        {
            _scenarioController.LoadNextScenario();
        }
    }
}