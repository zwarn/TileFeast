using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tools;
using UnityEngine;
using Zenject;

namespace UI.Tools
{
    public class ToolSelectionPanel : MonoBehaviour
    {
        [SerializeField] private Transform toolEntryParent;

        [SerializeField] [RequiredIn(PrefabKind.PrefabAsset)]
        private ToolSelectionEntry toolEntryPrefab;

        [Inject] private DiContainer _container;
        [Inject] private ToolController _toolController;

        private void Awake()
        {
            Clear();

            _toolController.AllToolData.ForEach(data =>
            {
                var entryObject = _container.InstantiatePrefab(toolEntryPrefab, toolEntryParent);
                var entry = entryObject.GetComponent<ToolSelectionEntry>();
                entry.SetData(data);
            });
        }

        private void Clear()
        {
            foreach (Transform child in toolEntryParent)
            {
                Destroy(child.gameObject);
            }
        }
    }
}