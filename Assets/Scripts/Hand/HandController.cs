using System;
using Piece.model;
using Piece.view;
using Scenario;
using State;
using UnityEngine;
using Zenject;

namespace Hand
{
    public class HandController : MonoBehaviour
    {
        [SerializeField] private PieceView pieceView;

        [Inject] private GameController _gameController;

        private PieceWithRotation _currentPiece;

        private void OnEnable()
        {
            _gameController.OnStateOverride += OnStateOverride;
        }

        private void OnDisable()
        {
            _gameController.OnStateOverride -= OnStateOverride;
        }

        private void OnStateOverride(GameState newState)
        {
            _currentPiece = newState.PieceInHand ? new PieceWithRotation(newState.PieceInHand, 0) : null;
            pieceView.SetData(_currentPiece);
        }

        private void Update()
        {
            var targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition.z = 0;
            transform.localPosition = targetPosition;
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