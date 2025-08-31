using Piece.model;
using Piece.ui;
using UnityEngine;

namespace Piece.view
{
    public class PieceView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private AspectListView aspectListView;

        private PieceWithRotation _piece;

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

        private void Update()
        {
            transform.rotation = Quaternion.Euler(0, 0, 90 * _piece.Rotation);
        }
    }
}