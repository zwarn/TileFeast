using Board;
using Core;
using Piece.Supply;
using UnityEngine;
using Zenject;

namespace Hand.Tool
{
    public class DrawAvailableTiles : DrawTool
    {
        [Inject] private GameController _gameController;
        [Inject] private BoardController _boardController;
        [Inject] private PieceSupplyController _pieceSupply;

        private GameState _gameState;

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
            _gameState = newState;
        }

        protected override void Paint(Vector2Int position)
        {
            if (!IsWithinBounds(position)) return;
            if (!_gameState.BlockedPositions.Contains(position)) return;

            _gameState.BlockedPositions.Remove(position);
            _gameController.TileChangedEvent(position);
        }

        protected override void Erase(Vector2Int position)
        {
            if (!IsWithinBounds(position)) return;
            if (_gameState.BlockedPositions.Contains(position)) return;

            // Return any piece at this position to supply
            var placedPiece = _boardController.GetPiece(position);
            if (placedPiece != null && !placedPiece.IsLocked())
            {
                _boardController.RemovePiece(placedPiece);
                _pieceSupply.AddPiece(placedPiece.Piece);
            }

            _gameState.BlockedPositions.Add(position);
            _gameController.TileChangedEvent(position);
        }

        private bool IsWithinBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x < _gameState.GridSize.x &&
                   position.y >= 0 && position.y < _gameState.GridSize.y;
        }
    }
}
