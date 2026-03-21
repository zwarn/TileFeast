using Rules;
using Scenarios;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI.Rules
{
    public class EmotionStatusPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text happyLabel;
        [SerializeField] private TMP_Text neutralLabel;
        [SerializeField] private TMP_Text sadLabel;
        [SerializeField] private TMP_Text scoreLabel;
        [SerializeField] private GameObject completionCheckmark;
        [SerializeField] private GameObject completionFailMark;

        [Inject] private RulesController _rulesController;
        [Inject] private ScenarioController _scenarioController;

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
            if (happyLabel != null) happyLabel.text = $"Happy: {result.HappyCount}";
            if (neutralLabel != null) neutralLabel.text = $"Neutral: {result.NeutralCount}";
            if (sadLabel != null) sadLabel.text = $"Sad: {result.SadCount}";
            if (scoreLabel != null) scoreLabel.text = $"Score: {result.Score}";

            bool complete = _rulesController.IsLevelComplete();
            if (completionCheckmark != null) completionCheckmark.SetActive(complete);
            if (completionFailMark != null) completionFailMark.SetActive(!complete);
        }

        public void GoToNextScenario()
        {
            _scenarioController.LoadNextScenario();
        }
    }
}
