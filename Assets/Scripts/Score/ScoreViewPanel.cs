using System;
using System.Collections.Generic;
using Scenario;
using UnityEngine;
using Zenject;

namespace Score
{
    public class ScoreViewPanel : MonoBehaviour
    {
        [Inject] private DiContainer _container;
        [Inject] private ScenarioController _scenarioController;

        [SerializeField] private Dictionary<ScoreCondition, ScoreViewEntry> _entries = new();
        [SerializeField] private ScoreViewEntry prefab;
        [SerializeField] private Transform parent;


        private void OnEnable()
        {
            _scenarioController.OnScenarioChanged += OnScenario;
        }

        private void OnDisable()
        {
            _scenarioController.OnScenarioChanged -= OnScenario;
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