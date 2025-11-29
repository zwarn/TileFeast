using Piece.Aspect;
using UnityEngine;

namespace Piece
{
    public class PieceView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private AspectListView aspectListView;

        private PieceWithRotation _piece;

        private void Update()
        {
            transform.rotation = Quaternion.Euler(0, 0, 90 * _piece.Rotation);
        }

        public void SetData(PieceWithRotation piece)
        {
            _piece = piece;
            gameObject.SetActive(_piece != null);

            if (_piece != null)
            {
                spriteRenderer.sprite = piece.Piece.sprite;
                transform.rotation = Quaternion.Euler(0, 0, 90 * _piece.Rotation);

                aspectListView.SetData(piece.Piece.aspects, piece.Piece);
            }
        }
    }
}