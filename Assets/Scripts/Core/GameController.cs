using System;
using Board;
using Piece;
using Piece.hand;
using Piece.Supply;
using Scenario;
using Score;
using UnityEngine;
using Zenject;

namespace Core
{
    public class GameController : MonoBehaviour
    {
        [Inject] private BoardController _boardController;

        [Inject] private HandController _handController;
        [Inject] private PieceSupplyController _pieceSupply;
        [Inject] private ScoreController _scoreController;

        public GameState CurrentState { get; private set; }

        private void Update()
        {
            HandleInput();
        }

        public event Action OnBoardChanged;

        public void LoadScenario(ScenarioSO scenario)
        {
            CurrentState = new GameState(scenario);

            //TODO: verify the new state is valid

            _boardController.UpdateState(CurrentState);
            _handController.UpdateState(CurrentState);
            _pieceSupply.UpdateState(CurrentState);
            _scoreController.UpdateState(CurrentState);

            BoardChangedEvent();
        }

        private void HandleInput()
        {
            if (Input.GetKeyUp(KeyCode.Q)) _handController.Rotate(1);

            if (Input.GetKeyUp(KeyCode.E)) _handController.Rotate(-1);

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

            _boardController.RemovePiece(placedPiece);
            _handController.SetPiece(new PieceWithRotation(placedPiece.Piece, placedPiece.Rotation));
            BoardChangedEvent();
        }

        public void GrabPieceFromSupply(PieceSO piece)
        {
            if (!_handController.IsEmpty()) return;

            _handController.SetPiece(new PieceWithRotation(piece, 0));
            _pieceSupply.RemovePiece(piece);
        }

        private void ReturnPieceToSupply()
        {
            if (_handController.IsEmpty()) return;

            var piece = _handController.GetPiece();
            _pieceSupply.AddPiece(piece);
            _handController.FreePiece();
        }

        public void BoardChangedEvent()
        {
            OnBoardChanged?.Invoke();
        }
    }
}