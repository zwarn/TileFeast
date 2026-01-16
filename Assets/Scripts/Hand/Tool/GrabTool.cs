using Board;
using Core;
using Piece;
using Piece.Supply;
using UnityEngine;
using Zenject;

namespace Hand.Tool
{
    public class GrabTool : MonoBehaviour, ITool
    {
        [SerializeField] private PieceView pieceView;
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
            pieceView.SetData(GetPiece());
        }

        public void OnSelect()
        {
            pieceView.SetData(GetPiece());
        }

        public void OnDeselect()
        {
            pieceView.SetData(null);
        }

        public void OnLeftClick(Vector2Int position)
        {
            if (GetPiece() == null)
                GrabFromBoard(position);
            else
                PutOnBoard(position);
        }

        public void OnRightClick(Vector2Int position)
        {
            if (IsEmpty()) return;

            var piece = GetPiece();
            _pieceSupply.AddPiece(piece);
            FreePiece();
            _gameController.BoardChangedEvent();
        }

        public void OnRotate(int direction)
        {
            if (!IsEmpty())
            {
                GetPiece().Rotate(direction);
            }
        }

        private void PutOnBoard(Vector2Int position)
        {
            var piece = GetPiece();
            var success = _boardController.PlacePiece(piece, position);
            if (success)
            {
                FreePiece();
                _gameController.BoardChangedEvent();
            }
        }

        private void GrabFromBoard(Vector2Int position)
        {
            if (GetPiece() != null) return;

            var placedPiece = _boardController.GetPiece(position);
            if (placedPiece == null) return;
            if (placedPiece.IsLocked()) return;

            _boardController.RemovePiece(placedPiece);
            SetPiece(new PieceWithRotation(placedPiece.Piece, placedPiece.Rotation));

            _gameController.BoardChangedEvent();
        }

        public bool IsEmpty()
        {
            return GetPiece() == null;
        }

        public void SetPiece(PieceWithRotation piece)
        {
            if (!IsEmpty())
            {
                Debug.LogError("Tried to override held piece");
                return;
            }

            _gameState.PieceInHand = piece;
            pieceView.SetData(GetPiece());
        }

        private PieceWithRotation GetPiece()
        {
            return _gameState.PieceInHand;
        }

        private void FreePiece()
        {
            _gameState.PieceInHand = null;
            pieceView.SetData(GetPiece());
        }

        public void GrabPieceFromSupply(Piece.Piece piece)
        {
            if (!IsEmpty()) return;

            SetPiece(new PieceWithRotation(piece, 0));
            _pieceSupply.RemovePiece(piece);
            _gameController.BoardChangedEvent();
        }

        public void ReturnPieceToSupply()
        {
            if (IsEmpty()) return;

            var piece = GetPiece();
            _pieceSupply.AddPiece(piece);
            FreePiece();
            _gameController.BoardChangedEvent();
        }
    }
}