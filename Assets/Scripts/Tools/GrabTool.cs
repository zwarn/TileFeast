using System.Collections.Generic;
using Board;
using BoardExpansion;
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
        [SerializeField] private BoardExpansionView boardExpansionView;
        [SerializeField] private Grid grid;

        [Inject] private GameController _gameController;
        [Inject] private BoardController _boardController;

        private bool _isSelected;
        private bool _processedMouseDown;
        private IPlaceable _currentPlaceable;

        private void OnEnable()
        {
            _gameController.OnHandChanged += UpdatePlaceable;
            _gameController.RegisterPieceHandFactory(
                piece => new PlaceablePiece(piece, pieceView, _gameController));
        }

        private void OnDisable()
        {
            _gameController.OnHandChanged -= UpdatePlaceable;
        }

        private void Update()
        {
            if (!_isSelected) return;

            var boardCell = GetBoardPosition();
            _currentPlaceable?.UpdatePreview(boardCell);

            if (!IsPointerOverSupplyPanel())
                HandleBoardInput();

            if (Input.GetMouseButtonUp(0))
                _processedMouseDown = false;

            HandleRotationInput();
        }

        private void HandleBoardInput()
        {
            var boardCell = GetBoardPosition();

            if (Input.GetMouseButtonDown(0))
            {
                _processedMouseDown = true;

                if (_currentPlaceable == null)
                {
                    var grabbed = _gameController.GrabPieceFromBoardInHand(boardCell);
                    if (grabbed != null)
                    {
                        var placeable = new PlaceablePiece(grabbed, pieceView, _gameController);
                        _gameController.SetItemInHand(placeable);
                    }
                }
                else
                {
                    bool success = _currentPlaceable.TryPlace(boardCell);
                    if (success) _gameController.ClearItemInHand();
                }
            }

            if (Input.GetMouseButtonUp(0) && !_processedMouseDown && _currentPlaceable != null)
            {
                bool success = _currentPlaceable.TryPlace(boardCell);
                if (success) _gameController.ClearItemInHand();
            }

            if (Input.GetMouseButtonDown(1) && _currentPlaceable != null)
                _gameController.DiscardItemInHand();
        }

        private void HandleRotationInput()
        {
            if (_currentPlaceable == null) return;

            var mouseScroll = Input.mouseScrollDelta.y;

            if (Input.GetKeyUp(KeyCode.Q) || mouseScroll > 0.5f)
                _currentPlaceable.Rotate(1);

            if (Input.GetKeyUp(KeyCode.E) || mouseScroll < -0.5f)
                _currentPlaceable.Rotate(-1);
        }

        public override void OnSelect()
        {
            _isSelected = true;
            UpdatePlaceable();
            gameObject.SetActive(true);
        }

        public override void OnDeselect()
        {
            _isSelected = false;
            if (!_gameController.IsHandEmpty())
                _gameController.DiscardItemInHand();

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
                    return true;
            }

            return false;
        }

        private void UpdatePlaceable()
        {
            if (_currentPlaceable != null)
                _currentPlaceable.PreviewObject.SetActive(false);

            _currentPlaceable = _gameController.GetItemInHand();

            if (_currentPlaceable != null)
                _currentPlaceable.PreviewObject.SetActive(true);
            else
                pieceView.SetData(null);
        }
    }
}
