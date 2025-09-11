using System;
using Scenario;
using UnityEngine;

namespace State
{
    public class GameStateController : MonoBehaviour
    {
        private GameState _gameCurrentState;

        public GameState CurrentState
        {
            get => _gameCurrentState;
            private set => _gameCurrentState = value;
        }

        public Action<GameState> OnStateOverride;

        public void LoadScenario(ScenarioSO scenario)
        {
            CurrentState = new GameState(scenario);
            OverrideStateEvent(CurrentState);
        }

        private void OverrideStateEvent(GameState newState)
        {
            OnStateOverride?.Invoke(newState);
        }
    }
}