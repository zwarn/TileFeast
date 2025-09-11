using System;
using System.Collections.Generic;
using Piece.model;
using Scenario;
using State;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Piece.controller
{
    public class PieceSupplyController : MonoBehaviour
    {
        [SerializeField] private List<PieceSO> pieces;

        public event Action<PieceSO> OnPieceAdded;
        public event Action<PieceSO> OnPieceRemoved;
        public event Action<List<PieceSO>> OnPiecesReplaced;

        [Inject] private GameStateController _gameStateController;

        private void OnEnable()
        {
            _gameStateController.OnStateOverride += OnStateOverride;
        }

        private void OnDisable()
        {
            _gameStateController.OnStateOverride -= OnStateOverride;
        }

        public void RemovePiece(PieceSO piece)
        {
            pieces.Remove(piece);
            RemovePieceEvent(piece);
        }

        public void AddPiece(PieceWithRotation piece)
        {
            pieces.Add(piece.Piece);
            AddPieceEvent(piece.Piece);
        }

        private void OnStateOverride(GameState newState)
        {
            pieces = newState.AvailablePieces;
            ReplacePiecesEvent(pieces);
        }

        public void RemovePieceEvent(PieceSO piece)
        {
            OnPieceRemoved?.Invoke(piece);
        }

        public void AddPieceEvent(PieceSO piece)
        {
            OnPieceAdded?.Invoke(piece);
        }

        public void ReplacePiecesEvent(List<PieceSO> newPieces)
        {
            OnPiecesReplaced?.Invoke(newPieces);
        }
    }
}
