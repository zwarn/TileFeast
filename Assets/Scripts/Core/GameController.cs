using System;
using System.Collections.Generic;
using Scenario;
using UnityEngine;

namespace Core
{
    public class GameController : MonoBehaviour
    {
        public event Action<GameState> OnChangeGameState;
        public event Action OnBoardChanged;
        public event Action<Vector2Int> OnTileChanged;

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
            ChangeGameStateEvent(CurrentState);
            BoardChangedEvent();
        }

        public void BoardChangedEvent()
        {
            OnBoardChanged?.Invoke();
        }

        public void TileChangedEvent(Vector2Int position)
        {
            OnTileChanged?.Invoke(position);
        }

        private void ChangeGameStateEvent(GameState newState)
        {
            OnChangeGameState?.Invoke(newState);
        }
    }
}
