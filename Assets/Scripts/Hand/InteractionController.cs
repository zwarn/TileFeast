using System;
using Board;
using Piece.controller;
using Piece.model;
using UnityEngine;
using Zenject;

namespace Hand
{
    public class InteractionController : MonoBehaviour
    {
        [Inject] private BoardController _boardController;
        [Inject] private HandController _handController;
        [Inject] private PieceSupplyController _pieceSupply;

        public event Action OnBoardChanged;

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetKeyUp(KeyCode.Q))
            {
                _handController.Rotate(1);
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                _handController.Rotate(-1);
            }

            if (Input.GetMouseButtonUp(1))
            {
                ReturnPieceToSupply();
            }
        }

        public void BoardClicked(Vector2Int position)
        {
            if (_handController.IsEmpty())
            {
                GrabFromBoard(position);
            }
            else
            {
                PutOnBoard(position);
            }
        }

        private void PutOnBoard(Vector2Int position)
        {
            var piece = _handController.GetPiece();
            bool success = _boardController.PlacePiece(piece, position);
            if (success)
            {
                _handController.FreePiece();
                BoardChangedEvent();
            }
        }

        private void GrabFromBoard(Vector2Int position)
        {
            if (!_handController.IsEmpty()) return;

            var placedPiece = _boardController.GetPiece(position);
            if (placedPiece == null) return;

            _boardController.RemovePiece(placedPiece);
            _handController.SetPiece(new PieceWithRotation(placedPiece.Piece, placedPiece.Rotation));
            BoardChangedEvent();
        }

        public void GrabPieceFromSupply(PieceSO piece)
        {
            if (!_handController.IsEmpty()) return;

            _handController.SetPiece(new PieceWithRotation(piece, 0));
            _pieceSupply.RemovePiece(piece);
        }

        private void ReturnPieceToSupply()
        {
            if (_handController.IsEmpty()) return;

            var piece = _handController.GetPiece();
            _pieceSupply.AddPiece(piece);
            _handController.FreePiece();
        }

        public void BoardChangedEvent()
        {
            OnBoardChanged?.Invoke();
        }
    }
}