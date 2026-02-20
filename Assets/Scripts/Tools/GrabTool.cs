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

        [Inject] private GameController _gameController;

        private bool _isSelected;
        private bool _processedMouseDown;

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

            // Handle board input when not over supply panel
            if (!IsPointerOverSupplyPanel())
            {
                HandleBoardInput();
            }

            // Reset flag on any mouse up (after processing)
            if (Input.GetMouseButtonUp(0))
            {
                _processedMouseDown = false;
            }

            HandleRotationInput();
        }

        private void HandleBoardInput()
        {
            // Click to grab or place
            if (Input.GetMouseButtonDown(0))
            {
                _processedMouseDown = true;

                if (_gameController.IsHandEmpty())
                {
                    var position = GetBoardPosition();
                    _gameController.GrabPieceFromBoardInHand(position);
                }
                else
                {
                    var position = GetBoardPosition();
                    _gameController.PutPieceInHandOnBoard(position);
                }
            }

            // Release over board with piece (for drag from UI to board)
            if (Input.GetMouseButtonUp(0) && !_processedMouseDown && !_gameController.IsHandEmpty())
            {
                var position = GetBoardPosition();
                _gameController.PutPieceInHandOnBoard(position);
            }

            if (Input.GetMouseButtonDown(1) && !_gameController.IsHandEmpty())
            {
                _gameController.ReturnPieceInHandToSupply();
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
