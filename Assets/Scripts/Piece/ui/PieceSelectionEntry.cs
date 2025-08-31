using Hand;
using Piece.model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Piece.ui
{
    public class PieceSelectionEntry : MonoBehaviour, IPointerClickHandler
    {
        [Inject] private InteractionController _interactionController;

        [SerializeField] private Image image;

        private PieceSO _piece;

        public void SetData(PieceSO piece)
        {
            _piece = piece;

            image.sprite = piece.sprite;
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            _interactionController.GrabPieceFromSupply(_piece);
        }
    }
}