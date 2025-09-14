using Core;
using UnityEngine;

namespace Piece.hand
{
    public class HandController : MonoBehaviour
    {
        [SerializeField] private PieceView pieceView;

        private PieceWithRotation _currentPiece;

        private void Update()
        {
            var targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition.z = 0;
            transform.localPosition = targetPosition;
        }

        public void UpdateState(GameState newState)
        {
            _currentPiece = newState.PieceInHand ? new PieceWithRotation(newState.PieceInHand, 0) : null;
            pieceView.SetData(_currentPiece);
        }

        public bool IsEmpty()
        {
            return _currentPiece == null;
        }

        public PieceWithRotation GetPiece()
        {
            return _currentPiece;
        }

        public void FreePiece()
        {
            _currentPiece = null;
            pieceView.SetData(_currentPiece);
        }

        public void SetPiece(PieceWithRotation piece)
        {
            if (!IsEmpty())
            {
                Debug.LogError("Tried to override held piece");
                return;
            }

            _currentPiece = piece;
            pieceView.SetData(_currentPiece);
        }

        public void Rotate(int direction)
        {
            _currentPiece.Rotate(direction);
        }
    }
}