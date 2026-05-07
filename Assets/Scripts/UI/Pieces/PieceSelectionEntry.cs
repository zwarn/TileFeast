using Board;
using Core;
using Pieces;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace UI.Pieces
{
    public class PieceSelectionEntry : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private static readonly Color HighlightTint = new(0.5f, 1f,   1f,   1f); // cyan tint
        private static readonly Color DimTint       = new(0.4f, 0.4f, 0.4f, 0.8f); // grey

        [SerializeField] private Image image;
        [Inject] private GameController      _gameController;
        [Inject] private ToolController      _toolController;
        [Inject] private HighlightService    _highlightService;

        private Piece _piece;

        private void OnEnable()
        {
            if (_highlightService != null)
            {
                _highlightService.OnSupplyHighlightChanged += ApplySupplyHighlight;
                _highlightService.OnSupplyHighlightCleared += ClearSupplyHighlight;
            }
        }

        private void OnDisable()
        {
            if (_highlightService != null)
            {
                _highlightService.OnSupplyHighlightChanged -= ApplySupplyHighlight;
                _highlightService.OnSupplyHighlightCleared -= ClearSupplyHighlight;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right &&
                _toolController.CurrentToolType == ToolType.RemovePieceTool)
            {
                _gameController.DeletePieceFromSupply(_piece);
                return;
            }

            _gameController.RequestGrabPieceFromSupply(_piece);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnPointerClick(eventData);
        }

        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData) { }

        public void SetData(Piece piece)
        {
            _piece = piece;
            image.sprite = piece.previewSprite != null ? piece.previewSprite : piece.sprite;
        }

        private void ApplySupplyHighlight(System.Predicate<Piece> predicate)
        {
            if (_piece == null || image == null) return;
            image.color = predicate(_piece) ? HighlightTint : DimTint;
        }

        private void ClearSupplyHighlight()
        {
            if (image != null) image.color = Color.white;
        }
    }
}
