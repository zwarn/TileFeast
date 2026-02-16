using Core;
using Pieces;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace UI.Pieces
{
    public class PieceSelectionEntry : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image image;
        [Inject] private GameController _gameController;

        private Piece _piece;

        public void OnPointerClick(PointerEventData eventData)
        {
            _gameController.RequestGrabPieceFromSupply(_piece);
        }

        public void SetData(Piece piece)
        {
            _piece = piece;
            image.sprite = piece.sprite;
        }
    }
}