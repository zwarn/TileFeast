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


        public void PutPieceInHandOnBoard(Vector2Int position)
        {
            var piece = GetPieceInHand();
            if (piece == null)
            {
                Debug.LogError("Tried PutPieceInHandOnBoard with empty hand");
                return;
            }

            var success = _boardController.PlacePiece(piece, position);
            if (success)
            {
                OnBoardChanged?.Invoke();
                ClearPieceInHand();
            }
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

        public void GrabPieceFromBoardInHand(Vector2Int position)
        {
            if (!IsHandEmpty())
            {
                Debug.LogError("Tried GrabPieceFromBoardInHand with piece in hand");
                return;
            }

            var placedPiece = _boardController.GetPiece(position);
            if (placedPiece == null) return;
            if (placedPiece.IsLocked()) return;

            _boardController.RemovePiece(placedPiece);
            OnBoardChanged?.Invoke();

            var piece = new PieceWithRotation(placedPiece.Piece, placedPiece.Rotation);
            SetPieceInHand(piece);
        }

        public void MovePieceFromSupplyToHand(Piece piece)
        {
            if (!IsHandEmpty())
            {
                Debug.LogError("Tried MovePieceFromSupplyToHand with piece in hand");
                return;
            }

            GrabPieceFromSupply(piece);
            SetPieceInHand(piece);
        }

        public void ReturnPieceInHandToSupply()
        {
            if (IsHandEmpty())
            {
                Debug.LogError("Tried ReturnPieceInHandToSupply with empty hand");
                return;
            }

            var piece = GetPieceInHand();
            ReturnPieceToSupply(piece);
            ClearPieceInHand();
        }

        private void ReturnPieceToSupply(PieceWithRotation piece)
        {
            if (piece == null) return;
            _pieceSupply.AddPiece(piece);
        }


        public PieceWithRotation GetPieceInHand()
        {
            return CurrentState?.PieceInHand;
        }

        public void RequestReturnPieceInHand()
        {
            if (_toolController.CurrentTool.Data.type != ToolType.GrabTool)
            {
                return;
            }

            if (IsHandEmpty())
            {
                return;
            }

            ReturnPieceInHandToSupply();
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
            {
                ReturnPieceInHandToSupply();
            }

            OnBoardChanged?.Invoke();
        }

        public void RequestGrabPieceFromSupply(Piece piece)
        {
            _toolController.ChangeTool(ToolType.GrabTool);

            if (!IsHandEmpty())
            {
                ReturnPieceInHandToSupply();
            }

            MovePieceFromSupplyToHand(piece);
        }

        private void SetPieceInHand(PieceWithRotation piece)
        {
            CurrentState.PieceInHand = piece;
            OnHandChanged?.Invoke();
        }

        private void SetPieceInHand(Piece piece)
        {
            SetPieceInHand(new PieceWithRotation(piece, 0));
        }

        private void ClearPieceInHand()
        {
            CurrentState.PieceInHand = null;
            OnHandChanged?.Invoke();
        }

        public bool IsHandEmpty()
        {
            return CurrentState?.PieceInHand == null;
        }


        private void GrabPieceFromSupply(Piece piece)
        {
            _pieceSupply.RemovePiece(piece);
        }


        private bool IsWithinBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x < CurrentState.GridSize.x &&
                   position.y >= 0 && position.y < CurrentState.GridSize.y;
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