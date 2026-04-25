using System;
using System.Collections.Generic;
using System.Linq;
using Board;
using BoardExpansion;
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
        [Inject] private BoardExpansionPreviewSettings _boardExpansionPreviewSettings;

        public event Action<GameState> OnChangeGameState;
        public event Action OnBoardChanged;
        public event Action<Vector2Int> OnTileChanged;
        public event Action OnHandChanged;

        private IPlaceable _itemInHand;

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

            var supplyItems = newState.AvailablePieces
                .Select(CreatePieceForSupply)
                .Where(item => item != null)
                .ToList();
            _pieceSupply.ReplaceItems(supplyItems);

            var expansionGenerator = new BoardExpansionPreviewGenerator(_boardExpansionPreviewSettings);
            _pieceSupply.AddItem(RandomBoardExpansionFactory.Create(expansionGenerator, this));

            _toolController.ChangeTool(ToolType.GrabTool);
            OnChangeGameState?.Invoke(CurrentState);
            OnBoardChanged?.Invoke();
        }

        public void LockTile(Vector2Int position, bool locked)
        {
            var pieceAtPosition = _boardController.GetPiece(position);
            if (pieceAtPosition == null) return;
            pieceAtPosition.Lock(locked);
        }

        public void BlockTile(Vector2Int position)
        {
            if (!IsWithinBounds(position)) return;
            if (CurrentState.BlockedPositions.Contains(position)) return;

            var placedPiece = _boardController.GetPiece(position);
            if (placedPiece != null)
            {
                if (placedPiece.IsLocked()) return;

                _boardController.RemovePiece(placedPiece);
                ReturnPieceToSupply(placedPiece.Piece);
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
                OnBoardChanged?.Invoke();
            return success;
        }

        public void DeletePieceFromBoard(Vector2Int position)
        {
            var placedPiece = _boardController.GetPiece(position);
            if (placedPiece == null) return;
            _boardController.RemovePiece(placedPiece);
        }

        // Removes a specific IPlaceable item from the supply permanently.
        public void DeleteFromSupply(IPlaceable item)
        {
            _pieceSupply.RemoveItem(item);
        }

        // Kept for SolverRunner and other callers that identify pieces by Piece reference.
        public void DeletePieceFromSupply(Piece piece)
        {
            var item = _pieceSupply.Items
                .OfType<PlaceablePiece>()
                .FirstOrDefault(pp => pp.Piece.Piece == piece);
            if (item != null) _pieceSupply.RemoveItem(item);
        }

        public void ClearPieceSupply()
        {
            _pieceSupply.DeleteAllItems();
        }

        // Grabs a piece from the board. Returns the PieceWithRotation on success (null if invalid or locked).
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

        // Sets any IPlaceable as the current in-hand item.
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

        // Grabs an IPlaceable from the supply and puts it in hand.
        public void RequestGrabFromSupply(IPlaceable item)
        {
            _toolController.ChangeTool(ToolType.GrabTool);
            if (!IsHandEmpty()) DiscardItemInHand();
            _pieceSupply.RemoveItem(item);
            SetItemInHand(item);
        }

        private IPlaceable CreatePieceForSupply(Piece piece)
            => new PlaceablePiece(new PieceWithRotation(piece, 0), this);

        public void ReturnToSupply(IPlaceable item)
            => _pieceSupply.AddItem(item);

        private void ReturnPieceToSupply(Piece piece)
            => _pieceSupply.AddItem(CreatePieceForSupply(piece));

        public void RequestReturnPieceInHand()
        {
            if (_toolController.CurrentTool.Data.type != ToolType.GrabTool) return;
            if (IsHandEmpty()) return;
            DiscardItemInHand();
        }

        public void ReturnPieceOnBoardToSupply(PlacedPiece piece)
        {
            _boardController.RemovePiece(piece);
            ReturnPieceToSupply(piece.Piece);
        }

        public void ReturnAllNonLockedPiecesToSupply()
        {
            var piecesToReturn = CurrentState.PlacedPieces
                .Where(p => !p.IsLocked())
                .ToList();

            foreach (var piece in piecesToReturn)
                ReturnPieceOnBoardToSupply(piece);

            if (!IsHandEmpty())
                DiscardItemInHand();

            OnBoardChanged?.Invoke();
        }

        public bool IsHandEmpty() => _itemInHand == null;

        private bool IsWithinBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x < CurrentState.GridSize.x &&
                   position.y >= 0 && position.y < CurrentState.GridSize.y;
        }

        public bool IsExpansionValid(List<Vector2Int> absoluteTiles)
        {
            foreach (var tile in absoluteTiles)
            {
                var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                foreach (var dir in dirs)
                    if (IsActiveTile(tile + dir)) return true;
            }
            return false;
        }

        private bool IsActiveTile(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < CurrentState.GridSize.x &&
                   pos.y >= 0 && pos.y < CurrentState.GridSize.y &&
                   !CurrentState.BlockedPositions.Contains(pos);
        }

        public void ExpandBoard(List<Vector2Int> absoluteTiles, List<Vector2Int> hWalls = null, List<Vector2Int> vWalls = null, List<Zone> zones = null)
        {
            var translate = _boardController.AddActiveTiles(absoluteTiles, CurrentState);
            _cameraController.HandleBoardResize(CurrentState.GridSize);
            _zoneController.HandleBoardResize(CurrentState.GridSize, Vector2Int.zero);
            _rulesController.HandleBoardResize(CurrentState.GridSize);

            if (hWalls != null)
                foreach (var w in hWalls)
                    AddHorizontalWall(w + translate);

            if (vWalls != null)
                foreach (var w in vWalls)
                    AddVerticalWall(w + translate);

            if (zones != null)
                foreach (var zone in zones)
                {
                    if (zone?.zoneType == null || zone.positions.Count == 0) continue;
                    Zone painted = null;
                    foreach (var pos in zone.positions)
                        painted = PaintZoneTile(pos + translate, zone.zoneType, painted);
                }

            OnBoardChanged?.Invoke();
        }

        public void ChangeBoardSize(Vector2Int deltaSize, Vector2Int translate)
        {
            var targetGridSize = (CurrentState.GridSize + deltaSize);
            if (targetGridSize.x <= 0 || targetGridSize.y <= 0) return;

            CurrentState.GridSize += deltaSize;

            _boardController.HandleBoardResize(CurrentState.GridSize, translate);
            _cameraController.HandleBoardResize(CurrentState.GridSize);
            _zoneController.HandleBoardResize(CurrentState.GridSize, translate);
            _rulesController.HandleBoardResize(CurrentState.GridSize);

            OnBoardChanged?.Invoke();
        }
    }
}
