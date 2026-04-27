using System.Collections.Generic;
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
        [SerializeField] private WallPlacementView wallPlacementView;
        [SerializeField] private ZonePlacementView zonePlacementView;
        [SerializeField] private Grid grid;

        [Inject] private GameController _gameController;

        private bool _isSelected;
        private bool _processedMouseDown;
        private IPlaceable _currentPlaceable;

        private void OnEnable()
        {
            _gameController.OnHandChanged += UpdatePlaceable;
        }

        private void OnDisable()
        {
            _gameController.OnHandChanged -= UpdatePlaceable;
        }

        private void Update()
        {
            if (!_isSelected) return;

            if (_currentPlaceable is BoardExpansion.BoardExpansion be)
            {
                var worldPos  = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var cell      = (Vector2Int)grid.WorldToCell(worldPos);
                var cellWorld = grid.CellToWorld(new Vector3Int(cell.x, cell.y, 0));
                boardExpansionView.transform.localPosition = new Vector3(
                    cellWorld.x + 1 - worldPos.x,
                    cellWorld.y + 1 - worldPos.y,
                    boardExpansionView.transform.localPosition.z);
                boardExpansionView.SetPreviewValid(be.IsValidPlacement(GetBoardPosition()));
            }
            else if (_currentPlaceable is BoardExpansion.WallPlacement wp)
            {
                var worldPos  = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var cell      = (Vector2Int)grid.WorldToCell(worldPos);
                var cellWorld = grid.CellToWorld(new Vector3Int(cell.x, cell.y, 0));
                wallPlacementView.transform.localPosition = new Vector3(
                    cellWorld.x + 1 - worldPos.x,
                    cellWorld.y + 1 - worldPos.y,
                    wallPlacementView.transform.localPosition.z);
                wallPlacementView.SetPreviewValid(wp.IsValidPlacement(GetBoardPosition()));
            }
            else if (_currentPlaceable is BoardExpansion.ZonePlacement zp)
            {
                var worldPos  = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var cell      = (Vector2Int)grid.WorldToCell(worldPos);
                var cellWorld = grid.CellToWorld(new Vector3Int(cell.x, cell.y, 0));
                zonePlacementView.transform.localPosition = new Vector3(
                    cellWorld.x + 1 - worldPos.x,
                    cellWorld.y + 1 - worldPos.y,
                    zonePlacementView.transform.localPosition.z);
                zonePlacementView.SetPreviewValid(zp.IsValidPlacement(GetBoardPosition()));
            }

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
                        _gameController.SetItemInHand(new PlaceablePiece(grabbed, _gameController));
                }
                else
                {
                    if (_currentPlaceable.TryPlace(boardCell))
                        _gameController.ClearItemInHand();
                }
            }

            if (Input.GetMouseButtonUp(0) && !_processedMouseDown && _currentPlaceable != null)
            {
                if (_currentPlaceable.TryPlace(boardCell))
                    _gameController.ClearItemInHand();
            }

            if (Input.GetMouseButtonDown(1) && _currentPlaceable != null)
                _gameController.DiscardItemInHand();
        }

        private void HandleRotationInput()
        {
            if (_currentPlaceable == null) return;

            var scroll = Input.mouseScrollDelta.y;
            int dir = 0;
            if (Input.GetKeyUp(KeyCode.Q) || scroll > 0.5f) dir = 1;
            else if (Input.GetKeyUp(KeyCode.E) || scroll < -0.5f) dir = -1;
            if (dir == 0) return;

            _currentPlaceable.Rotate(dir);

            if (_currentPlaceable is BoardExpansion.BoardExpansion be)
                boardExpansionView.SetData(be);
            else if (_currentPlaceable is BoardExpansion.WallPlacement wp)
                wallPlacementView.SetData(wp);
            else if (_currentPlaceable is BoardExpansion.ZonePlacement zp)
                zonePlacementView.SetData(zp);
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
            // Deactivate old preview
            if (_currentPlaceable is PlaceablePiece)
                pieceView.SetData(null);
            else if (_currentPlaceable is BoardExpansion.BoardExpansion)
            {
                boardExpansionView.transform.localPosition = Vector3.zero;
                boardExpansionView.gameObject.SetActive(false);
            }
            else if (_currentPlaceable is BoardExpansion.WallPlacement)
            {
                wallPlacementView.transform.localPosition = Vector3.zero;
                wallPlacementView.gameObject.SetActive(false);
            }
            else if (_currentPlaceable is BoardExpansion.ZonePlacement)
            {
                zonePlacementView.transform.localPosition = Vector3.zero;
                zonePlacementView.gameObject.SetActive(false);
            }

            _currentPlaceable = _gameController.GetItemInHand();

            // Activate new preview
            if (_currentPlaceable is PlaceablePiece pp)
                pieceView.SetData(pp.Piece);
            else if (_currentPlaceable is BoardExpansion.BoardExpansion be)
            {
                boardExpansionView.transform.localPosition = Vector3.zero;
                boardExpansionView.SetData(be);
                boardExpansionView.gameObject.SetActive(true);
            }
            else if (_currentPlaceable is BoardExpansion.WallPlacement wp2)
            {
                wallPlacementView.transform.localPosition = Vector3.zero;
                wallPlacementView.SetData(wp2);
                wallPlacementView.gameObject.SetActive(true);
            }
            else if (_currentPlaceable is BoardExpansion.ZonePlacement zp2)
            {
                zonePlacementView.transform.localPosition = Vector3.zero;
                zonePlacementView.SetData(zp2);
                zonePlacementView.gameObject.SetActive(true);
            }
            else
            {
                pieceView.SetData(null);
                boardExpansionView.gameObject.SetActive(false);
                wallPlacementView.gameObject.SetActive(false);
                zonePlacementView.gameObject.SetActive(false);
            }
        }
    }
}
