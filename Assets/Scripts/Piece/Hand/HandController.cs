using Core;
using UnityEngine;

namespace Piece.hand
{
    public class HandController : MonoBehaviour
    {
        [SerializeField] private PieceView pieceView;

        private GameState _gameState;

        private void Update()
        {
            var targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition.z = 0;
            transform.localPosition = targetPosition;
        }

        public void UpdateState(GameState newState)
        {
            _gameState = newState;
            pieceView.SetData(_gameState.PieceInHand);
        }

        public bool IsEmpty()
        {
            return _gameState.PieceInHand == null;
        }

        public PieceWithRotation GetPiece()
        {
            return _gameState.PieceInHand;
        }

        public void FreePiece()
        {
            _gameState.PieceInHand = null;
            pieceView.SetData(_gameState.PieceInHand);
        }

        public void SetPiece(PieceWithRotation piece)
        {
            if (!IsEmpty())
            {
                Debug.LogError("Tried to override held piece");
                return;
            }

            _gameState.PieceInHand = piece;
            pieceView.SetData(_gameState.PieceInHand);
        }

        public void Rotate(int direction)
        {
            _gameState.PieceInHand.Rotate(direction);
        }
    }
}