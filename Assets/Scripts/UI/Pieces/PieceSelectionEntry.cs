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
        private static readonly Color MatchColor = new(0f,   0.85f, 1f,   0.35f); // translucent cyan
        private static readonly Color DimColor   = new(0.1f, 0.1f,  0.1f, 0.45f); // translucent dark

        [SerializeField] private Image image;
        /// <summary>
        /// A white Image child covering the entry. Disabled by default; enabled and
        /// recolored when a supply highlight is active.
        /// </summary>
        [SerializeField] private Image highlightOverlay;

        [Inject] private GameController   _gameController;
        [Inject] private ToolController   _toolController;
        [Inject] private HighlightService _highlightService;

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
            ClearSupplyHighlight();
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

        public void OnBeginDrag(PointerEventData eventData) => OnPointerClick(eventData);
        public void OnDrag(PointerEventData eventData) { }
        public void OnEndDrag(PointerEventData eventData) { }

        public void SetData(Piece piece)
        {
            _piece = piece;
            image.sprite = piece.previewSprite != null ? piece.previewSprite : piece.sprite;
        }

        private void ApplySupplyHighlight(System.Predicate<Piece> predicate)
        {
            if (_piece == null || highlightOverlay == null) return;
            highlightOverlay.gameObject.SetActive(true);
            highlightOverlay.color = predicate(_piece) ? MatchColor : DimColor;
        }

        private void ClearSupplyHighlight()
        {
            if (highlightOverlay != null)
                highlightOverlay.gameObject.SetActive(false);
        }
    }
}
