﻿using System.Collections.Generic;
using Scenario;
using TMPro;
using UnityEngine;
using Zenject;

namespace Score
{
    public class ScoreViewPanel : MonoBehaviour
    {
        [SerializeField] private ScoreViewEntry prefab;
        [SerializeField] private Transform parent;
        [SerializeField] private TMP_Text total;

        private readonly Dictionary<ScoreRuleSO, ScoreViewEntry> _entries = new();
        [Inject] private DiContainer _container;
        [Inject] private ScenarioController _scenarioController;
        [Inject] private ScoreController _scoreController;

        private void Update()
        {
            total.text = _scoreController.TotalScore().ToString();
        }

        private void OnEnable()
        {
            _scoreController.OnScoreRuleReset += OnScoreRuleReset;
        }

        private void OnDisable()
        {
            _scoreController.OnScoreRuleReset -= OnScoreRuleReset;
        }

        private void OnScoreRuleReset(List<ScoreRuleSO> scoreRules)
        {
            Clear();
            scoreRules.ForEach(AddEntry);
        }

        public void GoToNextScenario()
        {
            _scenarioController.LoadNextScenario();
        }

        private void AddEntry(ScoreRuleSO rule)
        {
            var scoreViewGameobject = _container.InstantiatePrefab(prefab, parent);
            var scoreViewEntry = scoreViewGameobject.GetComponent<ScoreViewEntry>();
            scoreViewEntry.SetData(rule);
            _entries.Add(rule, scoreViewEntry);
        }

        private void Clear()
        {
            foreach (var scoreViewEntry in _entries.Values) Destroy(scoreViewEntry.gameObject);

            _entries.Clear();
        }
    }
}