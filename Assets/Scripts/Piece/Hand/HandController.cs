using Core;
using UnityEngine;
using Zenject;

namespace Piece.hand
{
    public class HandController : MonoBehaviour
    {
        [SerializeField] private PieceView pieceView;
        [Inject] private GameController _gameController;

        private GameState _gameState;

        
        private void OnEnable()
        {
            _gameController.OnChangeGameState += UpdateState;
        }

        private void OnDisable()
        {
            _gameController.OnChangeGameState -= UpdateState;
        }
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