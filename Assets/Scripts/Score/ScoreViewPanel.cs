using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Score
{
    public class ScoreViewPanel : MonoBehaviour
    {
        [Inject] private DiContainer _container;
        [Inject] private ScoreController _scoreController;

        [SerializeField] private Dictionary<ScoreCondition, ScoreViewEntry> _entries = new();
        [SerializeField] private ScoreViewEntry prefab;
        [SerializeField] private Transform parent;


        private void Start()
        {
            _scoreController.GetConditions().ForEach(AddEntry);
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