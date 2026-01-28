using System;
using System.Collections.Generic;
using Board;
using Hand.Tool;
using Piece;
using Piece.Supply;
using Scenario;
using UnityEngine;
using Zenject;

namespace Core
{
    public class GameController : MonoBehaviour
    {
        [Inject] private BoardController _boardController;
        [Inject] private PieceSupplyController _pieceSupply;
        [Inject] private ToolController _toolController;

        public event Action<GameState> OnChangeGameState;
        public event Action OnBoardChanged;
        public event Action<Vector2Int> OnTileChanged;
        public event Action OnHandChanged;

        public GameState CurrentState { get; private set; }

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

            CurrentState.BlockedPositions.Add(position);
            OnTileChanged?.Invoke(position);
        }

        public void UnblockTile(Vector2Int position)
        {
            if (!IsWithinBounds(position)) return;
            if (!CurrentState.BlockedPositions.Contains(position)) return;

            CurrentState.BlockedPositions.Remove(position);
            OnTileChanged?.Invoke(position);
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

        public void MovePieceFromSupplyToHand(Piece.Piece piece)
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

        public void RequestGrabPieceFromSupply(Piece.Piece piece)
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

        private void SetPieceInHand(Piece.Piece piece)
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


        private void GrabPieceFromSupply(Piece.Piece piece)
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
            CurrentState.GridSize += deltaSize;
            OnChangeGameState?.Invoke(CurrentState);
        }
    }
}