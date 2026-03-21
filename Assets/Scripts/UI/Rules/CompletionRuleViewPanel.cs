using System.Collections.Generic;
using Rules;
using Rules.CompletionRules;
using UnityEngine;
using Zenject;

namespace UI.Rules
{
    public class CompletionRuleViewPanel : MonoBehaviour
    {
        [SerializeField] private CompletionRuleViewEntry prefab;
        [SerializeField] private Transform parent;

        [Inject] private DiContainer _container;
        [Inject] private RulesController _rulesController;

        private readonly List<CompletionRuleViewEntry> _entries = new();

        private void OnEnable()
        {
            _rulesController.OnCompletionRulesReset += RebuildList;
            _rulesController.OnEvaluationChanged += RefreshEntries;
        }

        private void OnDisable()
        {
            _rulesController.OnCompletionRulesReset -= RebuildList;
            _rulesController.OnEvaluationChanged -= RefreshEntries;
        }

        private void RebuildList(List<CompletionRuleConfig> rules)
        {
            Clear();
            foreach (var config in rules)
            {
                var go = _container.InstantiatePrefab(prefab, parent);
                var entry = go.GetComponent<CompletionRuleViewEntry>();
                entry.SetRule(config, _rulesController);
                _entries.Add(entry);
            }
        }

        private void RefreshEntries(EmotionEvaluationResult result)
        {
            foreach (var entry in _entries)
                entry.Refresh();
        }

        private void Clear()
        {
            foreach (var entry in _entries)
                if (entry != null) Destroy(entry.gameObject);
            _entries.Clear();
        }
    }
}
