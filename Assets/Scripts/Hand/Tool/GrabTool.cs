using System;
using Core;
using Piece;
using UnityEngine;
using Zenject;

namespace Hand.Tool
{
    public class GrabTool : MonoBehaviour, ITool
    {
        [SerializeField] private PieceView pieceView;
        [SerializeField] private Grid grid;

        [Inject] private GameController _gameController;

        private bool _isSelected;
        private bool _isDragging;

        private void OnEnable()
        {
            _gameController.OnHandChanged += UpdatePieceView;
        }

        private void OnDisable()
        {
            _gameController.OnHandChanged -= UpdatePieceView;
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
                if (_gameController.IsHandEmpty())
                {
                    var position = GetBoardPosition();
                    _gameController.GrabPieceFromBoardInHand(position);
                    _isDragging = !_gameController.IsHandEmpty();
                }
            }

            // End drag or click - place piece
            if (Input.GetMouseButtonUp(0))
            {
                if (_isDragging)
                {
                    if (!_gameController.IsHandEmpty())
                    {
                        var position = GetBoardPosition();
                        _gameController.PutPieceInHandOnBoard(position);
                    }

                    _isDragging = false;
                }
                else if (!_gameController.IsHandEmpty())
                {
                    var position = GetBoardPosition();
                    _gameController.PutPieceInHandOnBoard(position);
                }
            }

            // Right click - return piece to supply
            if (Input.GetMouseButtonUp(1))
            {
                if (!_gameController.IsHandEmpty())
                {
                    _gameController.ReturnPieceInHandToSupply();
                    _isDragging = false;
                }
            }
        }

        private void HandleRotationInput()
        {
            var piece = _gameController.GetPieceInHand();
            if (piece == null) return;

            var mouseScroll = Input.mouseScrollDelta.y;

            if (Input.GetKeyUp(KeyCode.Q) || mouseScroll > 0.5f)
                piece.Rotate(1);

            if (Input.GetKeyUp(KeyCode.E) || mouseScroll < -0.5f)
                piece.Rotate(-1);
        }

        public void OnSelect()
        {
            _isSelected = true;
            pieceView.SetData(_gameController.GetPieceInHand());
            gameObject.SetActive(true);
        }

        public void OnDeselect()
        {
            _isSelected = false;
            _isDragging = false;
            if (!_gameController.IsHandEmpty())
            {
                _gameController.ReturnPieceInHandToSupply();
            }

            gameObject.SetActive(false);
        }

        private Vector2Int GetBoardPosition()
        {
            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return (Vector2Int)grid.WorldToCell(worldPos);
        }

        private void UpdatePieceView()
        {
            pieceView.SetData(_gameController.GetPieceInHand());
        }
    }
}