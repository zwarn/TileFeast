﻿using System.Collections.Generic;
using UI;
using UnityEngine;
using Zenject;

namespace Rules.Placement
{
    public class PlacementRuleViewPanel : MonoBehaviour
    {
        [SerializeField] private PlacementRuleViewEntry prefab;
        [SerializeField] private Transform parent;
        [SerializeField] private Checkmark checkmark;

        private readonly Dictionary<PlacementRuleSO, PlacementRuleViewEntry> _entries = new();
        [Inject] private DiContainer _container;
        [Inject] private RulesController rulesController;

        private void Update()
        {
            checkmark.SetState(rulesController.SatisfiesRules());
        }

        private void OnEnable()
        {
            rulesController.OnPlacementRuleReset += OnPlacementRuleReset;
        }

        private void OnDisable()
        {
            rulesController.OnPlacementRuleReset -= OnPlacementRuleReset;
        }

        private void OnPlacementRuleReset(List<PlacementRuleSO> placementRules)
        {
            Clear();
            placementRules.ForEach(AddEntry);
        }

        private void AddEntry(PlacementRuleSO rule)
        {
            var placementViewGameObject = _container.InstantiatePrefab(prefab, parent);
            var placementViewEntry = placementViewGameObject.GetComponent<PlacementRuleViewEntry>();
            placementViewEntry.SetData(rule);
            _entries.Add(rule, placementViewEntry);
        }

        private void Clear()
        {
            foreach (var scoreViewEntry in _entries.Values) Destroy(scoreViewEntry.gameObject);

            _entries.Clear();
        }
    }
}