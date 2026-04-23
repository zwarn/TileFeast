using System;
using System.Collections.Generic;
using System.Linq;
using Board;
using Zones;
using Tools;
using Cameras;
using Pieces;
using Pieces.Supply;
using Rules;
using Scenarios;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Core
{
    public class GameController : MonoBehaviour
    {
        [Inject] private BoardController _boardController;
        [Inject] private PieceSupplyController _pieceSupply;
        [Inject] private ToolController _toolController;
        [Inject] private ZoneController _zoneController;
        [Inject] private RulesController _rulesController;
        [Inject] private CameraController _cameraController;

        public event Action<GameState> OnChangeGameState;
        public event Action OnBoardChanged;
        public event Action<Vector2Int> OnTileChanged;
        public event Action OnHandChanged;

        private IPlaceable _itemInHand;
        private Func<PieceWithRotation, IPlaceable> _pieceInHandFactory;

        [ShowInInspector, ReadOnly] public GameState CurrentState { get; private set; }

        public void LoadScenario(ScenarioSO scenario)
        {
            var newState = new GameState(scenario);

            var validationResult = GameStateValidator.Validate(newState);
            if (!validationResult.IsValid)
            {
                Debug.LogError($"Invalid scenario '{scenario.name}':\n" +
                               string.Join("\n", (IEnumerable<string>)validationResult.Errors));
                return;
            }

            if (!IsHandEmpty())
                ClearItemInHand();

            CurrentState = newState;
            _toolController.ChangeTool(ToolType.GrabTool);
            OnChangeGameState?.Invoke(CurrentState);
            OnBoardChanged?.Invoke();
        }


        public void LockTile(Vector2Int position, bool locked)
        {
            var pieceAtPosition = _boardController.GetPiece(position);
            if (pieceAtPosition == null)
            {
                return;
            }

            pieceAtPosition.Lock(locked);
        }

        public void BlockTile(Vector2Int position)
        {
            if (!IsWithinBounds(position)) return;
            if (CurrentState.BlockedPositions.Contains(position)) return;

            // Remove any piece at this position and return to supply
            var placedPiece = _boardController.GetPiece(position);
            if (placedPiece != null)
            {
                if (placedPiece.IsLocked())
                {
                    return;
                }

                _boardController.RemovePiece(placedPiece);
                _pieceSupply.AddPiece(placedPiece.Piece);
            }

            _zoneController.RemoveTilesFromZones(new List<Vector2Int> { position });

            CurrentState.BlockedPositions.Add(position);
            OnTileChanged?.Invoke(position);
            OnBoardChanged?.Invoke();
        }

        public void UnblockTile(Vector2Int position)
        {
            if (!IsWithinBounds(position)) return;
            if (!CurrentState.BlockedPositions.Contains(position)) return;

            CurrentState.BlockedPositions.Remove(position);
            OnTileChanged?.Invoke(position);
            OnBoardChanged?.Invoke();
        }

        public void AddHorizontalWall(Vector2Int pos)
        {
            if (!IsValidHorizontalWallPosition(pos)) return;
            if (CurrentState.HorizontalWalls.Contains(pos)) return;
            if (AnyPlacedPieceCrossesHorizontalWall(pos)) return;

            CurrentState.HorizontalWalls.Add(pos);
            OnBoardChanged?.Invoke();
        }

        public void RemoveHorizontalWall(Vector2Int pos)
        {
            if (!CurrentState.HorizontalWalls.Remove(pos)) return;
            OnBoardChanged?.Invoke();
        }

        public void AddVerticalWall(Vector2Int pos)
        {
            if (!IsValidVerticalWallPosition(pos)) return;
            if (CurrentState.VerticalWalls.Contains(pos)) return;
            if (AnyPlacedPieceCrossesVerticalWall(pos)) return;

            CurrentState.VerticalWalls.Add(pos);
            OnBoardChanged?.Invoke();
        }

        public void RemoveVerticalWall(Vector2Int pos)
        {
            if (!CurrentState.VerticalWalls.Remove(pos)) return;
            OnBoardChanged?.Invoke();
        }

        private bool IsValidHorizontalWallPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < CurrentState.GridSize.x &&
                   pos.y >= 0 && pos.y < CurrentState.GridSize.y - 1;
        }

        private bool IsValidVerticalWallPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < CurrentState.GridSize.x - 1 &&
                   pos.y >= 0 && pos.y < CurrentState.GridSize.y;
        }

        private bool AnyPlacedPieceCrossesHorizontalWall(Vector2Int wallPos)
        {
            var above = new Vector2Int(wallPos.x, wallPos.y + 1);
            foreach (var piece in CurrentState.PlacedPieces)
            {
                var tiles = new HashSet<Vector2Int>(piece.GetTilePosition());
                if (tiles.Contains(wallPos) && tiles.Contains(above))
                    return true;
            }
            return false;
        }

        private bool AnyPlacedPieceCrossesVerticalWall(Vector2Int wallPos)
        {
            var right = new Vector2Int(wallPos.x + 1, wallPos.y);
            foreach (var piece in CurrentState.PlacedPieces)
            {
                var tiles = new HashSet<Vector2Int>(piece.GetTilePosition());
                if (tiles.Contains(wallPos) && tiles.Contains(right))
                    return true;
            }
            return false;
        }

        public Zone PaintZoneTile(Vector2Int position, ZoneSO zoneType, Zone zone)
        {
            if (!IsWithinBounds(position)) return zone;
            if (CurrentState.BlockedPositions.Contains(position)) return zone;

            if (zone == null)
            {
                zone = new Zone(zoneType, new List<Vector2Int>());
                CurrentState.Zones.Add(zone);
            }

            _zoneController.AddTilesToZone(zone, new List<Vector2Int> { position });
            CurrentState.Zones.RemoveAll(z => z.positions.Count == 0);
            OnBoardChanged?.Invoke();
            return zone;
        }

        public void EraseZoneTile(Vector2Int position)
        {
            if (!IsWithinBounds(position)) return;

            _zoneController.RemoveTilesFromZones(new List<Vector2Int> { position });
            CurrentState.Zones.RemoveAll(z => z.positions.Count == 0);
            OnBoardChanged?.Invoke();
        }


        // Called by PlaceablePiece.TryPlace — places piece on board, returns success.
        public bool PlacePieceInHand(PieceWithRotation piece, Vector2Int position)
        {
            var success = _boardController.PlacePiece(piece, position);
            if (success)
                OnBoardChanged?.Invoke();
            return success;
        }

        public bool SpawnPiece(Piece piece, Vector2Int position, int rotation)
        {
            var rotatedPiece = new PieceWithRotation(piece, rotation);

            var success = _boardController.PlacePiece(rotatedPiece, position);
            if (success)
            {
                OnBoardChanged?.Invoke();
            }

            return success;
        }

        public void DeletePieceFromBoard(Vector2Int position)
        {
            var placedPiece = _boardController.GetPiece(position);
            if (placedPiece == null)
            {
                return;
            }
            
            _boardController.RemovePiece(placedPiece);
        }

        public void DeletePieceFromSupply(Piece piece)
        {
            _pieceSupply.RemovePiece(piece);
        }

        public void ClearPieceSupply()
        {
            _pieceSupply.DeleteAllPieces();
        }

        // Grabs a piece from the board and fires OnHandChanged so GrabTool can wrap it.
        // Returns the PieceWithRotation on success (null if invalid or locked).
        public PieceWithRotation GrabPieceFromBoardInHand(Vector2Int position)
        {
            if (!IsHandEmpty()) return null;

            var placedPiece = _boardController.GetPiece(position);
            if (placedPiece == null) return null;
            if (placedPiece.IsLocked()) return null;

            _boardController.RemovePiece(placedPiece);
            OnBoardChanged?.Invoke();
            return new PieceWithRotation(placedPiece.Piece, placedPiece.Rotation);
        }

        // Removes piece from supply; fires OnHandChanged so GrabTool can wrap it.
        // Returns the PieceWithRotation ready to be held.
        public PieceWithRotation GrabPieceFromSupplyForHand(Piece piece)
        {
            _pieceSupply.RemovePiece(piece);
            return new PieceWithRotation(piece, 0);
        }

        // Sets any IPlaceable as the current in-hand item (used by GrabTool and BoardExpansion supply).
        public void SetItemInHand(IPlaceable item)
        {
            _itemInHand = item;
            CurrentState.HasPieceInHand = item is PlaceablePiece;
            OnHandChanged?.Invoke();
        }

        public IPlaceable GetItemInHand() => _itemInHand;

        // Clears hand state without calling OnDiscard (used after successful placement).
        public void ClearItemInHand()
        {
            _itemInHand = null;
            CurrentState.HasPieceInHand = false;
            OnHandChanged?.Invoke();
        }

        // Discards the current in-hand item: clears state, calls OnDiscard for cleanup.
        public void DiscardItemInHand()
        {
            if (_itemInHand == null) return;
            var item = _itemInHand;
            _itemInHand = null;
            CurrentState.HasPieceInHand = false;
            item.OnDiscard();
            OnHandChanged?.Invoke();
        }

        // Adds a piece back to the supply (called by PlaceablePiece.OnDiscard).
        public void ReturnPieceToSupply(Piece piece) => _pieceSupply.AddPiece(piece);

        public void RequestReturnPieceInHand()
        {
            if (_toolController.CurrentTool.Data.type != ToolType.GrabTool) return;
            if (IsHandEmpty()) return;
            DiscardItemInHand();
        }

        public void ReturnPieceOnBoardToSupply(PlacedPiece piece)
        {
            _boardController.RemovePiece(piece);
            _pieceSupply.AddPiece(piece.Piece);
        }

        public void ReturnAllNonLockedPiecesToSupply()
        {
            var piecesToReturn = CurrentState.PlacedPieces
                .Where(p => !p.IsLocked())
                .ToList();

            foreach (var piece in piecesToReturn)
            {
                ReturnPieceOnBoardToSupply(piece);
            }

            if (!IsHandEmpty())
                DiscardItemInHand();

            OnBoardChanged?.Invoke();
        }

        public void RequestGrabPieceFromSupply(Piece piece)
        {
            _toolController.ChangeTool(ToolType.GrabTool);
            if (!IsHandEmpty()) DiscardItemInHand();
            var pieceWithRotation = GrabPieceFromSupplyForHand(piece);
            var placeable = _pieceInHandFactory?.Invoke(pieceWithRotation);
            if (placeable != null) SetItemInHand(placeable);
        }

        // GrabTool registers this factory so GameController can create PlaceablePiece
        // without knowing about PieceView.
        public void RegisterPieceHandFactory(Func<PieceWithRotation, IPlaceable> factory)
        {
            _pieceInHandFactory = factory;
        }

        public bool IsHandEmpty() => _itemInHand == null;


        private bool IsWithinBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x < CurrentState.GridSize.x &&
                   position.y >= 0 && position.y < CurrentState.GridSize.y;
        }

        public void ExpandBoard(List<Vector2Int> absoluteTiles)
        {
            _boardController.AddActiveTiles(absoluteTiles, CurrentState);
            _cameraController.HandleBoardResize(CurrentState.GridSize);
            _zoneController.HandleBoardResize(CurrentState.GridSize, Vector2Int.zero);
            _rulesController.HandleBoardResize(CurrentState.GridSize);
            OnBoardChanged?.Invoke();
        }

        public void ChangeBoardSize(Vector2Int deltaSize, Vector2Int translate)
        {
            var targetGridSize = (CurrentState.GridSize + deltaSize);
            if (targetGridSize.x <= 0 || targetGridSize.y <= 0)
            {
                return;
            }

            CurrentState.GridSize += deltaSize;

            _boardController.HandleBoardResize(CurrentState.GridSize, translate);
            _cameraController.HandleBoardResize(CurrentState.GridSize);
            _zoneController.HandleBoardResize(CurrentState.GridSize, translate);
            _rulesController.HandleBoardResize(CurrentState.GridSize);

            OnBoardChanged?.Invoke();
        }
    }
}