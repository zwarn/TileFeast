using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Piece;
using UnityEngine;
using Zenject;

namespace Board
{
    public class BoardController : MonoBehaviour
    {
        private int width;
        private int height;
        private readonly Dictionary<Vector2Int, PlacedPiece> _piecesByPosition = new();

        private List<PlacedPiece> _pieces = new();
        private List<Vector2Int> _blockPositions = new();

        public List<PlacedPiece> Pieces => _pieces.ToList();

        [Inject] private GameController _gameController;

        public event Action<List<PlacedPiece>> OnBoardReset;
        public event Action<PlacedPiece> OnPiecePlaced;
        public event Action<PlacedPiece> OnPieceRemoved;

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
            _pieces = newState.PlacedPieces;
            _blockPositions = newState.BlockedPositions;
            ResetBoardSize(newState.GridSize);
            RebuildPiecesByPosition();
            BoardResetEvent(Pieces);
        }

        private void ResetBoardSize(Vector2Int gridSize)
        {
            width = gridSize.x;
            height = gridSize.y;
        }

        private void RebuildPiecesByPosition()
        {
            _piecesByPosition.Clear();
            _pieces.ForEach(piece => piece.GetTilePosition().ForEach(pos => _piecesByPosition[pos] = piece));
        }

        public bool PlacePiece(PieceWithRotation newPiece, Vector2Int position)
        {
            var piece = new PlacedPiece(newPiece.Piece, newPiece.Rotation, position);
            if (!IsValid(piece.GetTilePosition())) return false;

            _pieces.Add(piece);
            piece.GetTilePosition().ForEach(pos => _piecesByPosition[pos] = piece);
            PlacePieceEvent(piece);
            return true;
        }

        public bool RemovePiece(PlacedPiece piece)
        {
            var removed = _pieces.Remove(piece);
            if (!removed) return false;

            piece.GetTilePosition().ForEach(pos => _piecesByPosition.Remove(pos));
            RemovePieceEvent(piece);
            return true;
        }

        public PlacedPiece GetPiece(Vector2Int position)
        {
            if (_piecesByPosition.TryGetValue(position, out var piece)) return piece;

            return null;
        }

        public Dictionary<Vector2Int, PlacedPiece> GetPieceByPosition()
        {
            return _piecesByPosition.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private bool IsValid(List<Vector2Int> tiles)
        {
            return tiles.TrueForAll(IsValid);
        }

        private bool IsValid(Vector2Int position)
        {
            return InBounds(position) && !IsBlocked(position) && IsEmpty(position);
        }

        private bool IsBlocked(Vector2Int position)
        {
            return _blockPositions.Contains(position);
        }

        private bool InBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x < width &&
                   position.y >= 0 && position.y < height;
        }

        private bool IsEmpty(Vector2Int position)
        {
            _piecesByPosition.TryGetValue(position, out var piece);
            return piece == null;
        }

        private void PlacePieceEvent(PlacedPiece piece)
        {
            OnPiecePlaced?.Invoke(piece);
        }

        private void RemovePieceEvent(PlacedPiece piece)
        {
            OnPieceRemoved?.Invoke(piece);
        }

        private void BoardResetEvent(List<PlacedPiece> placedPieces)
        {
            OnBoardReset?.Invoke(placedPieces);
        }
    }
}