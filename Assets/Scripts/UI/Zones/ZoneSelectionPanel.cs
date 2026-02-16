using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Zones;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace UI.Zones
{
    public class ZoneSelectionPanel : MonoBehaviour
    {
        [SerializeField] private Transform zoneEntryParent;
        [SerializeField] private Transform visualParent;

        [SerializeField] [RequiredIn(PrefabKind.PrefabAsset)]
        private ZoneSelectionEntry zoonEntryPrefab;

        [SerializeField] private List<ZoneSO> allZones;

        [Inject] private DiContainer _container;
        [Inject] private ToolController _toolController;

        private void Awake()
        {
            Clear();

            GetAllZones().ForEach(data =>
            {
                var entryObject = _container.InstantiatePrefab(zoonEntryPrefab, zoneEntryParent);
                var entry = entryObject.GetComponent<ZoneSelectionEntry>();
                entry.SetData(data);
            });
        }

        private void OnEnable()
        {
            _toolController.OnToolChanged += UpdateVisibility;
        }

        private void OnDisable()
        {
            _toolController.OnToolChanged -= UpdateVisibility;
        }

        private void UpdateVisibility(ToolType toolType)
        {
            visualParent.gameObject.SetActive(toolType == ToolType.ZonesTool);
        }

        private List<ZoneSO> GetAllZones()
        {
            return allZones.ToList();
        }

        private void Clear()
        {
            foreach (Transform child in zoneEntryParent)
            {
                Destroy(child.gameObject);
            }
        }
    }
}