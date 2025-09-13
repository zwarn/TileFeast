using System.Collections.Generic;
using Scenario;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Score
{
    public class ScoreViewPanel : MonoBehaviour
    {
        [Inject] private DiContainer _container;
        [Inject] private ScoreController _scoreController;
        [Inject] private ScenarioController _scenarioController;
        
        [SerializeField] private ScoreViewEntry prefab;
        [SerializeField] private Transform parent;
        [SerializeField] private TMP_Text total;

        private Dictionary<ScoreRule, ScoreViewEntry> _entries = new();

        private void OnEnable()
        {
            _scoreController.OnScoreRuleReset += OnScoreRuleReset;
        }

        private void OnDisable()
        {
            _scoreController.OnScoreRuleReset -= OnScoreRuleReset;
        }

        private void Update()
        {
            total.text = _scoreController.TotalScore().ToString();
        }

        private void OnScoreRuleReset(List<ScoreRule> scoreRules)
        {
            Clear();
            scoreRules.ForEach(AddEntry);
        }

        public void GoToNextScenario()
        {
            _scenarioController.LoadNextScenario();
        }

        private void AddEntry(ScoreRule rule)
        {
            var scoreViewGameobject = _container.InstantiatePrefab(prefab, parent);
            var scoreViewEntry = scoreViewGameobject.GetComponent<ScoreViewEntry>();
            scoreViewEntry.SetData(rule);
            _entries.Add(rule, scoreViewEntry);
        }

        private void Clear()
        {
            foreach (var scoreViewEntry in _entries.Values)
            {
                Destroy(scoreViewEntry.gameObject);
            }

            _entries.Clear();
        }
    }
}