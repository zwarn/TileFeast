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

        [SerializeField] private Dictionary<ScoreCondition, ScoreViewEntry> _entries = new();
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
            scenario.scoreConditions.ForEach(AddEntry);
        }

        private void AddEntry(ScoreCondition condition)
        {
            var conditionViewGameobject = _container.InstantiatePrefab(prefab, parent);
            var scoreViewEntry = conditionViewGameobject.GetComponent<ScoreViewEntry>();
            scoreViewEntry.SetData(condition);
            _entries.Add(condition, scoreViewEntry);
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