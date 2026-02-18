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
        [SerializeField] private Image image;
        [Inject] private GameController _gameController;
        [Inject] private ToolController _toolController;

        private Piece _piece;

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
            image.sprite = piece.sprite;
        }
    }
}