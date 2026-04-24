using Core;
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

        private IPlaceable _item;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right &&
                _toolController.CurrentToolType == ToolType.RemovePieceTool)
            {
                _gameController.DeleteFromSupply(_item);
                return;
            }

            _gameController.RequestGrabFromSupply(_item);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnPointerClick(eventData);
        }

        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData) { }

        public void SetData(IPlaceable item)
        {
            _item = item;
            image.sprite = item.PreviewSprite;
        }
    }
}
