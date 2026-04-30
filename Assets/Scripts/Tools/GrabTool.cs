using System;
using System.Collections.Generic;
using Core;
using Pieces;
using Placeables.BoardExpansions;
using Placeables.PersonalRulePlacements;
using Placeables.WallPlacements;
using Placeables.ZonePlacementS;
using Roguelike;
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
        [SerializeField] private PersonalRulePlacementView personalRulePlacementView;
        [SerializeField] private Grid grid;

        [Inject] private GameController _gameController;

        private bool _isSelected;
        private bool _processedMouseDown;
        private IPlaceable _currentPlaceable;
        private IPlaceableView _activeView;
        private Dictionary<Type, IPlaceableView> _viewMap;

        private void Awake()
        {
            _viewMap = new Dictionary<Type, IPlaceableView>
            {
                { typeof(PlaceablePiece),          pieceView },
                { typeof(BoardExpansion),           boardExpansionView },
                { typeof(WallPlacement),            wallPlacementView },
                { typeof(ZonePlacement),            zonePlacementView },
                { typeof(PersonalRulePlacement),    personalRulePlacementView },
            };

            foreach (var view in _viewMap.Values)
                view.Deactivate();
        }

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

            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _activeView?.UpdateFrame(grid, worldPos, GetBoardPosition());

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
            _activeView?.OnRotated();
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
            _activeView?.Deactivate();

            _currentPlaceable = _gameController.GetItemInHand();

            var forView = _currentPlaceable is DraftPlaceable draft ? draft.Inner : _currentPlaceable;

            if (forView != null && _viewMap.TryGetValue(forView.GetType(), out var view))
            {
                _activeView = view;
                _activeView.Bind(forView);
                _activeView.Activate();
            }
            else
            {
                _activeView = null;
            }
        }
    }
}
