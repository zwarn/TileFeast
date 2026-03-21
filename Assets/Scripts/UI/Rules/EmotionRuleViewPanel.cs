using System.Collections.Generic;
using Rules;
using Rules.EmotionRules;
using UnityEngine;
using Zenject;

namespace UI.Rules
{
    public class EmotionRuleViewPanel : MonoBehaviour
    {
        [SerializeField] private EmotionRuleViewEntry prefab;
        [SerializeField] private Transform parent;

        [Inject] private DiContainer _container;
        [Inject] private RulesController _rulesController;

        private readonly List<EmotionRuleViewEntry> _entries = new();

        private void OnEnable()
        {
            _rulesController.OnEmotionRulesReset += RebuildList;
        }

        private void OnDisable()
        {
            _rulesController.OnEmotionRulesReset -= RebuildList;
        }

        private void RebuildList(List<EmotionRuleConfig> rules)
        {
            Clear();
            foreach (var config in rules)
            {
                var go = _container.InstantiatePrefab(prefab, parent);
                var entry = go.GetComponent<EmotionRuleViewEntry>();
                entry.SetRule(config);
                _entries.Add(entry);
            }
        }

        private void Clear()
        {
            foreach (var entry in _entries)
                if (entry != null) Destroy(entry.gameObject);
            _entries.Clear();
        }
    }
}
