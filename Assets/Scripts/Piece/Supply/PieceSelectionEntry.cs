using Core;
using Hand.Tool;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Piece.Supply
{
    public class PieceSelectionEntry : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image image;
        [Inject] private ToolController _toolController;

        private Piece _piece;


        public void OnPointerClick(PointerEventData eventData)
        {
            var grabTool = _toolController.SelectGrabTool();
            grabTool.GrabPieceFromSupply(_piece);
        }

        public void SetData(Piece piece)
        {
            _piece = piece;

            image.sprite = piece.sprite;
        }
    }
}