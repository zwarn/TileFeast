using System.Collections.Generic;
using Core;
using Hand.Tool;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Piece.Supply
{
    public class PieceSelectionPanel : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler,
        IDragHandler, IDropHandler
    {
        [SerializeField] private PieceSelectionEntry prefab;
        [SerializeField] private Transform entryParent;

        private readonly Dictionary<Piece, PieceSelectionEntry> _entries = new();
        [Inject] private DiContainer _container;
        [Inject] private PieceSupplyController _pieceSupply;
        [Inject] private GameController _gameController;
        [Inject] private ToolController _toolController;

        public GraphicRaycaster raycaster;

        private void OnEnable()
        {
            _pieceSupply.OnPieceAdded += PieceAdded;
            _pieceSupply.OnPieceRemoved += PieceRemoved;
            _pieceSupply.OnPiecesReplaced += PiecesReplaced;
        }

        private void OnDisable()
        {
            _pieceSupply.OnPieceAdded -= PieceAdded;
            _pieceSupply.OnPieceRemoved -= PieceRemoved;
            _pieceSupply.OnPiecesReplaced -= PiecesReplaced;
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

        public PieceSelectionEntry GetPieceSelectionEntryPointedAt()
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            foreach (var r in results)
            {
                PieceSelectionEntry entry = r.gameObject.GetComponentInParent<PieceSelectionEntry>();
                if (entry != null)
                    return entry;
            }

            return null;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var pieceSelection = GetPieceSelectionEntryPointedAt();
            if (pieceSelection != null)
            {
                pieceSelection.OnPointerClick(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!_toolController.IsHoldingGrabTool())
            {
                return;
            }

            var grabTool = _toolController.grabTool;
            grabTool.ReturnPieceToSupply();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_toolController.IsHoldingGrabTool())
            {
                return;
            }

            var grabTool = _toolController.grabTool;
            grabTool.ReturnPieceToSupply();
        }
    }
}