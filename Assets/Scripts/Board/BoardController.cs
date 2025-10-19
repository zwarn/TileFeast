using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Piece;
using UnityEngine;
using UnityEngine.Serialization;

namespace Board
{
    public class BoardController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider2D boxCollider;
        private int width;
        private int height;
        private readonly Dictionary<Vector2Int, PlacedPiece> _piecesByPosition = new();

        private List<PlacedPiece> _pieces = new();

        public List<PlacedPiece> Pieces => _pieces.ToList();

        public event Action<List<PlacedPiece>> OnBoardReset;
        public event Action<PlacedPiece> OnPiecePlaced;
        public event Action<PlacedPiece> OnPieceRemoved;

        public void UpdateState(GameState newState)
        {
            _pieces = newState.PlacedPieces;
            ResetBoardSize(newState.GridSize);
            RebuildPiecesByPosition();
            BoardResetEvent(Pieces);
        }

        private void ResetBoardSize(Vector2Int gridSize)
        {
            width = gridSize.x;
            height = gridSize.y;

            transform.position = new Vector3((width - 1) / 2, (height - 1) / 2, 0);

            spriteRenderer.size = gridSize;
            boxCollider.size = gridSize;

            var maxSize = Math.Max(width, height);

            var camera = Camera.main;
            camera.orthographicSize = 1f + maxSize / 2f;
            camera.transform.position = new Vector3((width - 1) / 2, (height - 1) / 2, camera.transform.position.z);
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
            return InBounds(position) && IsEmpty(position);
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