using System;
using System.Collections.Generic;
using Core;
using UnityEngine;
using Zenject;

namespace Piece.Supply
{
    public class PieceSupplyController : MonoBehaviour
    {
        private List<Piece> pieces;
        [Inject] private GameController _gameController;

        public event Action<Piece> OnPieceAdded;
        public event Action<Piece> OnPieceRemoved;
        public event Action<List<Piece>> OnPiecesReplaced;

        private void OnEnable()
        {
            _gameController.OnChangeGameState += UpdateState;
        }

        private void OnDisable()
        {
            _gameController.OnChangeGameState -= UpdateState;
        }
        
        public void UpdateState(GameState newState)
        {
            pieces = newState.AvailablePieces;
            ReplacePiecesEvent(pieces);
        }

        public void RemovePiece(Piece piece)
        {
            pieces.Remove(piece);
            RemovePieceEvent(piece);
        }

        public void AddPiece(PieceWithRotation piece)
        {
            AddPiece(piece.Piece);
        }

        public void AddPiece(Piece piece)
        {
            pieces.Add(piece);
            AddPieceEvent(piece);
        }

        public void RemovePieceEvent(Piece piece)
        {
            OnPieceRemoved?.Invoke(piece);
        }

        public void AddPieceEvent(Piece piece)
        {
            OnPieceAdded?.Invoke(piece);
        }

        public void ReplacePiecesEvent(List<Piece> newPieces)
        {
            OnPiecesReplaced?.Invoke(newPieces);
        }
    }
}