using Pieces;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace UI.Pieces
{
    public class PieceCreationEntry : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image image;
        [Inject] private ShapeTool _shapeTool;

        private PieceMatch _pieceMatch;

        public void OnPointerClick(PointerEventData eventData)
        {
            _shapeTool.SpawnPiece(_pieceMatch);
        }

        public void SetData(PieceMatch match)
        {
            _pieceMatch = match;
            image.sprite = match.Piece.sprite;
        }
    }
}