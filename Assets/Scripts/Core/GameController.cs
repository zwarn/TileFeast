using System;
using System.Collections.Generic;
using Board;
using Hand.Tool;
using Piece.Supply;
using Scenario;
using UnityEngine;
using Zenject;

namespace Core
{
    public class GameController : MonoBehaviour
    {
        [Inject] private BoardController _boardController;

        [Inject] private ToolController _toolController;
        [Inject] private PieceSupplyController _pieceSupply;

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

            var validationResult = GameStateValidator.Validate(newState);
            if (!validationResult.IsValid)
            {
                Debug.LogError($"Invalid scenario '{scenario.name}':\n" +
                               string.Join("\n", (IEnumerable<string>)validationResult.Errors));
                return;
            }

            CurrentState = newState;
            ChangeGameStateEvent(CurrentState);
            BoardChangedEvent();
        }

        private void HandleInput()
        {
            var mouseScroll = Input.mouseScrollDelta.y;
            
            if (Input.GetKeyUp(KeyCode.Q) || mouseScroll > 0.5f) _toolController.Rotate(1);

            if (Input.GetKeyUp(KeyCode.E) || mouseScroll < -0.5f) _toolController.Rotate(-1);

            if (Input.GetMouseButtonUp(1)) _toolController.RightClicked(Vector2Int.zero);
        }

        public void BoardClicked(Vector2Int position)
        {
            _toolController.LeftClicked(position);
        }

        public void BoardChangedEvent()
        {
            OnBoardChanged?.Invoke();
        }

        private void ChangeGameStateEvent(GameState newState)
        {
            OnChangeGameState?.Invoke(newState);
        }
    }
}