using System;
using System.Collections.Generic;
using System.Linq;
using Piece.model;
using Scenario;
using UnityEngine;
using Zenject;

namespace Board
{
    public class BoardController : MonoBehaviour
    {
        [SerializeField] public int width;
        [SerializeField] public int height;

        private readonly List<PlacedPiece> _pieces = new();
        private readonly Dictionary<Vector2Int, PlacedPiece> _piecesByPosition = new();

        [Inject] private ScenarioController _scenarioController;

        public event Action<PlacedPiece> OnPiecePlaced;
        public event Action<PlacedPiece> OnPieceRemoved;

        public List<PlacedPiece> Pieces => _pieces.ToList();

        private void OnEnable()
        {
            _scenarioController.OnScenarioChanged += OnScenario;
        }

        private void OnDisable()
        {
            _scenarioController.OnScenarioChanged -= OnScenario;
        }

        private void OnScenario(ScenarioSO scenario)
        {
            Clear();
        }

        private void Clear()
        {
            _pieces.ForEach(piece => RemovePiece(piece));
        }
        
        public bool PlacePiece(PieceWithRotation newPiece, Vector2Int position)
        {
            var piece = new PlacedPiece(newPiece.Piece, newPiece.Rotation, position);
            if (!IsValid(piece.GetTilePosition()))
            {
                return false;
            }

            _pieces.Add(piece);
            piece.GetTilePosition().ForEach(pos => _piecesByPosition[pos] = piece);
            PlacePieceEvent(piece);
            return true;
        }

        public bool RemovePiece(PlacedPiece piece)
        {
            bool removed = _pieces.Remove(piece);
            if (!removed)
            {
                return false;
            }

            piece.GetTilePosition().ForEach(pos => _piecesByPosition.Remove(pos));
            RemovePieceEvent(piece);
            return true;
        }

        public PlacedPiece GetPiece(Vector2Int position)
        {
            if (_piecesByPosition.TryGetValue(position, out var piece))
            {
                return piece;
            }

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
    }
}