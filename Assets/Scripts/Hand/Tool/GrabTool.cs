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
        [SerializeField] private Grid grid;

        [Inject] private GameController _gameController;
        [Inject] private BoardController _boardController;
        [Inject] private PieceSupplyController _pieceSupply;

        private GameState _gameState;
        private bool _isSelected;
        private bool _isDragging;

        private void OnEnable()
        {
            _gameController.OnChangeGameState += UpdateState;
        }

        private void OnDisable()
        {
            _gameController.OnChangeGameState -= UpdateState;
        }

        private void Update()
        {
            if (!_isSelected) return;

            HandleClickInput();
            HandleRotationInput();
        }

        private void HandleClickInput()
        {
            // Start drag - grab piece from board
            if (Input.GetMouseButtonDown(0))
            {
                if (IsEmpty())
                {
                    var position = GetBoardPosition();
                    GrabFromBoard(position);
                    _isDragging = !IsEmpty(); // Started dragging if we grabbed something
                }
            }

            // End drag or click - place piece
            if (Input.GetMouseButtonUp(0))
            {
                if (_isDragging)
                {
                    // End of drag - place piece
                    if (!IsEmpty())
                    {
                        var position = GetBoardPosition();
                        PutOnBoard(position);
                    }
                    _isDragging = false;
                }
                else if (!IsEmpty())
                {
                    // Regular click while holding - place piece
                    var position = GetBoardPosition();
                    PutOnBoard(position);
                }
            }

            // Right click - return piece to supply
            if (Input.GetMouseButtonUp(1))
            {
                if (!IsEmpty())
                {
                    ReturnPieceToSupply();
                    _isDragging = false;
                }
            }
        }

        private void HandleRotationInput()
        {
            if (IsEmpty()) return;

            var mouseScroll = Input.mouseScrollDelta.y;

            if (Input.GetKeyUp(KeyCode.Q) || mouseScroll > 0.5f)
                GetPiece().Rotate(1);

            if (Input.GetKeyUp(KeyCode.E) || mouseScroll < -0.5f)
                GetPiece().Rotate(-1);
        }

        public void UpdateState(GameState newState)
        {
            _gameState = newState;
            pieceView.SetData(GetPiece());
        }

        public void OnSelect()
        {
            _isSelected = true;
            pieceView.SetData(GetPiece());
            gameObject.SetActive(true);
        }

        public void OnDeselect()
        {
            _isSelected = false;
            _isDragging = false;
            if (!IsEmpty())
            {
                ReturnPieceToSupply();
            }
            gameObject.SetActive(false);
        }

        private Vector2Int GetBoardPosition()
        {
            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return (Vector2Int)grid.WorldToCell(worldPos);
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
            return _gameState?.PieceInHand;
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
