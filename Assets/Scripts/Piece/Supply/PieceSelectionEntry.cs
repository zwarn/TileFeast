using Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Piece.Supply
{
    public class PieceSelectionEntry : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image image;
        [Inject] private GameController _gameController;

        private PieceSO _piece;


        public void OnPointerClick(PointerEventData eventData)
        {
            _gameController.GrabPieceFromSupply(_piece);
        }

        public void SetData(PieceSO piece)
        {
            _piece = piece;

            image.sprite = piece.sprite;
        }
    }
}