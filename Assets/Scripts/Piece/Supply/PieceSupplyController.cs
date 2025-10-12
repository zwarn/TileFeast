using System;
using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Piece.Supply
{
    public class PieceSupplyController : MonoBehaviour
    {
        private List<Piece> pieces;

        public event Action<Piece> OnPieceAdded;
        public event Action<Piece> OnPieceRemoved;
        public event Action<List<Piece>> OnPiecesReplaced;

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
            pieces.Add(piece.Piece);
            AddPieceEvent(piece.Piece);
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