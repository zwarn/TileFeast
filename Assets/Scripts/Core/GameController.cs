using System;
using Board;
using Piece;
using Piece.hand;
using Piece.Supply;
using Rules;
using Scenario;
using UnityEngine;
using Zenject;

namespace Core
{
    public class GameController : MonoBehaviour
    {
        [Inject] private BoardController _boardController;

        [Inject] private HandController _handController;
        [Inject] private PieceSupplyController _pieceSupply;
        [Inject] private RulesController rulesController;

        public event Action<GameState> OnChangeGameState;
        public event Action OnBoardChanged;

        public GameState CurrentState { get; private set; }

        private void Update()
        {
            HandleInput();
        }


        public void LoadScenario(ScenarioSO scenario)
        {
            var newState = new GameState(scenario);

            //TODO: verify the new state is valid

            CurrentState = newState;
            ChangeGameStateEvent(CurrentState);
            BoardChangedEvent();
        }

        private void HandleInput()
        {
            var mouseScroll = Input.mouseScrollDelta.y;
            
            if (Input.GetKeyUp(KeyCode.Q) || mouseScroll > 0.5f) _handController.Rotate(1);

            if (Input.GetKeyUp(KeyCode.E) || mouseScroll < -0.5f) _handController.Rotate(-1);

            if (Input.GetMouseButtonUp(1)) ReturnPieceToSupply();
        }

        public void BoardClicked(Vector2Int position)
        {
            if (_handController.IsEmpty())
                GrabFromBoard(position);
            else
                PutOnBoard(position);
        }

        private void PutOnBoard(Vector2Int position)
        {
            var piece = _handController.GetPiece();
            var success = _boardController.PlacePiece(piece, position);
            if (success)
            {
                _handController.FreePiece();
                BoardChangedEvent();
            }
        }

        private void GrabFromBoard(Vector2Int position)
        {
            if (!_handController.IsEmpty()) return;

            var placedPiece = _boardController.GetPiece(position);
            if (placedPiece == null) return;
            if (placedPiece.IsLocked()) return;

            _boardController.RemovePiece(placedPiece);
            _handController.SetPiece(new PieceWithRotation(placedPiece.Piece, placedPiece.Rotation));

            BoardChangedEvent();
        }

        public void GrabPieceFromSupply(Piece.Piece piece)
        {
            if (!_handController.IsEmpty()) return;

            _handController.SetPiece(new PieceWithRotation(piece, 0));
            _pieceSupply.RemovePiece(piece);
            BoardChangedEvent();
        }

        public void ReturnPieceToSupply()
        {
            if (_handController.IsEmpty()) return;

            var piece = _handController.GetPiece();
            _pieceSupply.AddPiece(piece);
            _handController.FreePiece();
            BoardChangedEvent();
        }

        public void BoardChangedEvent()
        {
            OnBoardChanged?.Invoke();
        }

        public void ChangeGameStateEvent(GameState newState)
        {
            OnChangeGameState?.Invoke(newState);
        }
    }
}