using System.Collections.Generic;
using Core;
using Pieces;
using UI.Pieces;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Tools
{
    public class GrabTool : ToolBase
    {
        [SerializeField] private PieceView pieceView;
        [SerializeField] private Grid grid;
        [SerializeField] private float dragThreshold = 0.5f; // Grid cells moved before considered a drag

        [Inject] private GameController _gameController;

        private bool _isSelected;
        private bool _isDragging;
        private bool _mouseDown;
        private bool _grabbedThisClick;
        private Vector2Int _mouseDownPosition;

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
            // Start potential drag - grab piece from board
            if (Input.GetMouseButtonDown(0))
            {
                _mouseDownPosition = GetBoardPosition();
                _mouseDown = true;
                _isDragging = false;
                _grabbedThisClick = false;

                if (_gameController.IsHandEmpty())
                {
                    _gameController.GrabPieceFromBoardInHand(_mouseDownPosition);
                    _grabbedThisClick = !_gameController.IsHandEmpty();
                }
            }

            // Check if we've moved enough to consider it a drag
            if (_mouseDown && !_isDragging && !_gameController.IsHandEmpty())
            {
                var currentPosition = GetBoardPosition();
                var distance = Vector2Int.Distance(_mouseDownPosition, currentPosition);
                if (distance >= dragThreshold)
                {
                    _isDragging = true;
                }
            }

            // End drag or click - place piece
            if (Input.GetMouseButtonUp(0))
            {
                // Check if releasing over supply panel - return to supply
                if (IsPointerOverSupplyPanel() && !_gameController.IsHandEmpty())
                {
                    _gameController.ReturnPieceInHandToSupply();
                }
                else if (_isDragging)
                {
                    // Was a drag - place piece at current position
                    if (!_gameController.IsHandEmpty())
                    {
                        var position = GetBoardPosition();
                        _gameController.PutPieceInHandOnBoard(position);
                    }
                }
                else if (!_grabbedThisClick)
                {
                    // Click to place (hand already had a piece before this click)
                    if (!_gameController.IsHandEmpty())
                    {
                        var position = GetBoardPosition();
                        _gameController.PutPieceInHandOnBoard(position);
                    }
                }
                // else: was a click to pick up without dragging - keep in hand

                _isDragging = false;
                _mouseDown = false;
                _grabbedThisClick = false;
            }

            // Right click - return piece to supply
            if (Input.GetMouseButtonUp(1))
            {
                if (!_gameController.IsHandEmpty())
                {
                    _gameController.ReturnPieceInHandToSupply();
                    _isDragging = false;
                    _mouseDown = false;
                    _grabbedThisClick = false;
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

        public override void OnSelect()
        {
            _isSelected = true;
            pieceView.SetData(_gameController.GetPieceInHand());
            gameObject.SetActive(true);
        }

        public override void OnDeselect()
        {
            _isSelected = false;
            _isDragging = false;
            _mouseDown = false;
            _grabbedThisClick = false;
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

        private bool IsPointerOverSupplyPanel()
        {
            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if (result.gameObject.GetComponentInParent<PieceSelectionPanel>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdatePieceView()
        {
            pieceView.SetData(_gameController.GetPieceInHand());
        }
    }
}