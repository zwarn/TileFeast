using System.Collections.Generic;
using Core;
using Pieces;
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

        private readonly Dictionary<Piece, PieceSelectionEntry> _entries = new();
        [Inject] private DiContainer _container;
        [Inject] private PieceSupplyController _pieceSupply;
        [Inject] private ToolController _toolController;
        [Inject] private GameController _gameController;

        private void OnEnable()
        {
            _pieceSupply.OnPieceAdded += PieceAdded;
            _pieceSupply.OnPieceRemoved += PieceRemoved;
            _pieceSupply.OnPiecesReplaced += PiecesReplaced;
            _toolController.OnToolChanged += UpdateVisibility;
        }

        private void OnDisable()
        {
            _pieceSupply.OnPieceAdded -= PieceAdded;
            _pieceSupply.OnPieceRemoved -= PieceRemoved;
            _pieceSupply.OnPiecesReplaced -= PiecesReplaced;
            _toolController.OnToolChanged -= UpdateVisibility;
        }

        private void UpdateVisibility(ToolType toolType)
        {
            visualParent.gameObject.SetActive(toolType != ToolType.ZonesTool && toolType != ToolType.PiecesTool);
        }

        private void PiecesReplaced(List<Piece> pieces)
        {
            foreach (var entry in _entries) Destroy(entry.Value.gameObject);
            _entries.Clear();
            pieces.ForEach(PieceAdded);
        }

        private void PieceRemoved(Piece piece)
        {
            var entry = _entries[piece];
            Destroy(entry.gameObject);
            _entries.Remove(piece);
        }

        private void PieceAdded(Piece piece)
        {
            var entryObject = _container.InstantiatePrefab(prefab, entryParent);
            var entry = entryObject.GetComponent<PieceSelectionEntry>();
            entry.SetData(piece);
            _entries.Add(piece, entry);
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