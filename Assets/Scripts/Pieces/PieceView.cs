using Pieces.Aspects;
using UnityEngine;

namespace Pieces
{
    public class PieceView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private AspectListView aspectListView;

        private PieceWithRotation _piece;

        private void Update()
        {
            transform.rotation = Quaternion.Euler(0, 0, 90 * _piece?.Rotation ?? 0);
        }

        private void OnDestroy()
        {
            if (_piece != null)
            {
                _piece.Piece.OnChanged -= OnPieceChanged;
            }
        }

        public void SetData(PieceWithRotation piece)
        {
            if (_piece != null)
            {
                _piece.Piece.OnChanged -= OnPieceChanged;
            }

            _piece = piece;
            gameObject.SetActive(_piece != null);

            if (_piece != null)
            {
                _piece.Piece.OnChanged += OnPieceChanged;
                spriteRenderer.sprite = piece.Piece.sprite;
                transform.rotation = Quaternion.Euler(0, 0, 90 * _piece.Rotation);

                aspectListView.SetData(piece.Piece);
            }
        }

        private void OnPieceChanged()
        {
            if (_piece != null)
            {
                aspectListView.SetData(_piece.Piece);
            }
        }
    }
}