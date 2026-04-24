using System.Collections.Generic;
using Core;
using Pieces.Supply;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace UI.Pieces
{
    public class PieceSelectionPanel : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler,
        IDragHandler, IDropHandler
    {
        [SerializeField] private PieceSelectionEntry prefab;
        [SerializeField] private Transform entryParent;
        [SerializeField] private Transform visualParent;

        private readonly Dictionary<IPlaceable, PieceSelectionEntry> _entries = new();
        [Inject] private DiContainer _container;
        [Inject] private PieceSupplyController _pieceSupply;
        [Inject] private ToolController _toolController;
        [Inject] private GameController _gameController;

        private void OnEnable()
        {
            _pieceSupply.OnItemAdded += ItemAdded;
            _pieceSupply.OnItemRemoved += ItemRemoved;
            _pieceSupply.OnItemsReplaced += ItemsReplaced;
            _toolController.OnToolChanged += UpdateVisibility;
        }

        private void OnDisable()
        {
            _pieceSupply.OnItemAdded -= ItemAdded;
            _pieceSupply.OnItemRemoved -= ItemRemoved;
            _pieceSupply.OnItemsReplaced -= ItemsReplaced;
            _toolController.OnToolChanged -= UpdateVisibility;
        }

        private void UpdateVisibility(ToolType toolType)
        {
            visualParent.gameObject.SetActive(
                toolType != ToolType.ZonesTool &&
                toolType != ToolType.PiecesTool &&
                toolType != ToolType.CalculateTool);
        }

        private void ItemsReplaced(List<IPlaceable> items)
        {
            foreach (var entry in _entries) Destroy(entry.Value.gameObject);
            _entries.Clear();
            items.ForEach(ItemAdded);
        }

        private void ItemRemoved(IPlaceable item)
        {
            if (!_entries.TryGetValue(item, out var entry)) return;
            Destroy(entry.gameObject);
            _entries.Remove(item);
        }

        private void ItemAdded(IPlaceable item)
        {
            var entryObject = _container.InstantiatePrefab(prefab, entryParent);
            var entry = entryObject.GetComponent<PieceSelectionEntry>();
            entry.SetData(item);
            _entries.Add(item, entry);
        }

        public void OnBeginDrag(PointerEventData eventData) { }
        public void OnEndDrag(PointerEventData eventData) { }
        public void OnDrag(PointerEventData eventData) { }
        public void OnDrop(PointerEventData eventData) { }

        public void OnPointerClick(PointerEventData eventData)
        {
            _gameController.RequestReturnPieceInHand();
        }
    }
}
