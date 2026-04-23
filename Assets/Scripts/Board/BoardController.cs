using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Pieces;
using UnityEngine;
using Zenject;

namespace Board
{
    public class BoardController : MonoBehaviour
    {
        private int _width;
        private int _height;
        private readonly Dictionary<Vector2Int, PlacedPiece> _piecesByPosition = new();

        private List<PlacedPiece> _pieces = new();
        private List<Vector2Int> _blockPositions = new();
        private List<Vector2Int> _horizontalWalls = new();
        private List<Vector2Int> _verticalWalls = new();

        public List<PlacedPiece> Pieces => _pieces.ToList();

        [Inject] private GameController _gameController;

        public event Action<List<PlacedPiece>> OnBoardReset;
        public event Action<PlacedPiece> OnPiecePlaced;
        public event Action<PlacedPiece> OnPieceRemoved;
        public event Action<List<PlacedPiece>, Vector2Int> OnPiecesMoved;
        public event Action OnBoardResize;

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
            _horizontalWalls = newState.HorizontalWalls;
            _verticalWalls = newState.VerticalWalls;
            ResetBoardSize(newState.GridSize);
            RebuildPiecesByPosition();
            BoardResetEvent(Pieces);
        }

        public void HandleBoardResize(Vector2Int newSize, Vector2Int translate)
        {
            ResetBoardSize(newSize);

            if (translate.magnitude > 0)
            {
                MovePieces(translate);
                MoveBlockedPositions(translate);
                MoveWalls(_horizontalWalls, translate);
                MoveWalls(_verticalWalls, translate);
            }

            // Remove blocked positions that are now out of bounds
            _blockPositions.RemoveAll(pos => !InBounds(pos));

            // Remove walls that are now out of bounds
            _horizontalWalls.RemoveAll(w => w.x < 0 || w.x >= _width || w.y < 0 || w.y >= _height - 1);
            _verticalWalls.RemoveAll(w => w.x < 0 || w.x >= _width - 1 || w.y < 0 || w.y >= _height);

            // Rebuild position dictionary BEFORE validation so RemovePiece works correctly
            RebuildPiecesByPosition();

            // Remove pieces that are now out of bounds or on blocked positions
            var piecesToRemove = _pieces
                .Where(piece => !piece.GetTilePosition().TrueForAll(pos => InBounds(pos) && !IsBlocked(pos)))
                .ToList();

            foreach (var piece in piecesToRemove)
            {
                _gameController.ReturnPieceOnBoardToSupply(piece);
            }

            OnBoardResize?.Invoke();
        }

        // Expands the board to include the given absolute tile positions.
        // Extends GridSize to the new bounding box, calls HandleBoardResize to translate
        // existing pieces/walls, then reconciles blocked positions so exactly the
        // union of old active tiles and newTiles is unblocked.
        public void AddActiveTiles(List<Vector2Int> newTiles, GameState state)
        {
            if (newTiles == null || newTiles.Count == 0) return;

            // 1. Capture current active set before any resize.
            var currentActive = new HashSet<Vector2Int>();
            for (int x = 0; x < _width; x++)
                for (int y = 0; y < _height; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (!_blockPositions.Contains(pos))
                        currentActive.Add(pos);
                }

            // 2. Compute bounding box over current active + new tiles.
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var p in currentActive.Concat(newTiles))
            {
                if (p.x < minX) minX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.x > maxX) maxX = p.x;
                if (p.y > maxY) maxY = p.y;
            }

            // 3. Compute translate and new size.
            var translate = new Vector2Int(-minX, -minY);
            var newSize = new Vector2Int(maxX - minX + 1, maxY - minY + 1);
            state.GridSize = newSize;

            // 4. Build the final active set (translated).
            var finalActive = new HashSet<Vector2Int>(
                currentActive.Select(p => p + translate)
                    .Concat(newTiles.Select(p => p + translate)));

            // 5. Call HandleBoardResize to translate pieces/walls/blocks and fire OnBoardResize.
            HandleBoardResize(newSize, translate);

            // 6. Reconcile blocked positions for the new bounding box.
            for (int x = 0; x < newSize.x; x++)
            {
                for (int y = 0; y < newSize.y; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (finalActive.Contains(pos))
                        _blockPositions.Remove(pos);
                    else if (!_blockPositions.Contains(pos))
                        _blockPositions.Add(pos);
                }
            }

            // 7. Re-fire so BoardView redraws the updated block layout.
            OnBoardResize?.Invoke();
        }

        private void MoveBlockedPositions(Vector2Int translate)
        {
            for (int i = 0; i < _blockPositions.Count; i++)
                _blockPositions[i] += translate;
        }

        private void MoveWalls(List<Vector2Int> walls, Vector2Int translate)
        {
            for (int i = 0; i < walls.Count; i++)
                walls[i] += translate;
        }

        private void MovePieces(Vector2Int translate)
        {
            Pieces.ForEach(piece => { piece.Move(translate); });
            OnPiecesMoved?.Invoke(Pieces, translate);
        }

        private void ResetBoardSize(Vector2Int gridSize)
        {
            _width = gridSize.x;
            _height = gridSize.y;
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

            piece.Lock(false);
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

        public bool IsValid(List<Vector2Int> tiles)
        {
            return tiles.TrueForAll(IsValid) && !CrossesWall(tiles);
        }

        private bool CrossesWall(List<Vector2Int> tiles)
        {
            var tileSet = new HashSet<Vector2Int>(tiles);
            foreach (var t in tiles)
            {
                if (tileSet.Contains(new Vector2Int(t.x, t.y + 1)) && _horizontalWalls.Contains(t))
                    return true;
                if (tileSet.Contains(new Vector2Int(t.x + 1, t.y)) && _verticalWalls.Contains(t))
                    return true;
            }
            return false;
        }

        public bool IsValid(Vector2Int position)
        {
            return InBounds(position) && !IsBlocked(position) && IsEmpty(position);
        }

        private bool IsBlocked(Vector2Int position)
        {
            return _blockPositions.Contains(position);
        }

        private bool InBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x < _width &&
                   position.y >= 0 && position.y < _height;
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