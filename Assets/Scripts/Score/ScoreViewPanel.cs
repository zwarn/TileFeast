using System;
using System.Collections.Generic;
using Board;
using Scenario;
using TMPro;
using UnityEngine;
using Zenject;

namespace Score
{
    public class ScoreViewPanel : MonoBehaviour
    {
        [Inject] private DiContainer _container;
        [Inject] private ScenarioController _scenarioController;
        [Inject] private ScoreController _scoreController;

        [SerializeField] private Dictionary<ScoreRule, ScoreViewEntry> _entries = new();
        [SerializeField] private ScoreViewEntry prefab;
        [SerializeField] private Transform parent;
        [SerializeField] private TMP_Text total;


        private void OnEnable()
        {
            _scenarioController.OnScenarioChanged += OnScenario;
        }

        private void OnDisable()
        {
            _scenarioController.OnScenarioChanged -= OnScenario;
        }

        private void Update()
        {
            total.text = _scoreController.TotalScore().ToString();
        }

        private void OnScenario(ScenarioSO scenario)
        {
            Clear();
            scenario.scoreRules.ForEach(AddEntry);
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