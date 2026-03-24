using System;
using System.Collections.Generic;
using System.Linq;
using Board;
using Core;
using Rules.CompletionRules;
using Rules.EmotionRules;
using Sirenix.Utilities;
using UnityEngine;
using Zenject;
using Zones;

namespace Rules
{
    public class RulesController : MonoBehaviour
    {
        [Inject] private BoardController _boardController;
        [Inject] private GameController _gameController;
        [Inject] private ZoneController _zoneController;

        public event Action<EmotionEvaluationResult> OnEvaluationChanged;
        public event Action<List<EmotionRuleConfig>> OnEmotionRulesReset;
        public event Action<List<CompletionRuleConfig>> OnCompletionRulesReset;

        private List<EmotionRuleConfig> _emotionRules = new();
        private List<CompletionRuleConfig> _completionRules = new();
        private Vector2Int _gridSize;

        public EmotionEvaluationResult LastResult { get; private set; } = EmotionEvaluationResult.Empty();

        private void OnEnable()
        {
            _gameController.OnChangeGameState += HandleStateChanged;
            _gameController.OnBoardChanged += Evaluate;
        }

        private void OnDisable()
        {
            _gameController.OnChangeGameState -= HandleStateChanged;
            _gameController.OnBoardChanged -= Evaluate;
        }

        public bool IsLevelComplete()
        {
            if (_completionRules.IsNullOrEmpty())
            {
                return true;
            }

            return _completionRules.All(c => c.rule.IsMet(LastResult, _gameController.CurrentState));
        }

        public int TotalScore() => LastResult.Score;

        public void HandleBoardResize(Vector2Int size)
        {
            _gridSize = size;
            Evaluate();
            OnEmotionRulesReset?.Invoke(_emotionRules);
            OnCompletionRulesReset?.Invoke(_completionRules);
        }

        private void HandleStateChanged(GameState gameState)
        {
            _emotionRules = gameState.EmotionRules;
            _completionRules = gameState.CompletionRules;
            _gridSize = gameState.GridSize;
            Evaluate();
            OnEmotionRulesReset?.Invoke(_emotionRules);
            OnCompletionRulesReset?.Invoke(_completionRules);
        }

        private void Evaluate()
        {
            if (_gameController.CurrentState == null) return;

            var state = _gameController.CurrentState;
            var tileDict = _boardController.GetPieceByPosition();
            var tileArray = RulesHelper.ConvertTiles(tileDict, _gridSize.x, _gridSize.y);
            var context = new EmotionContext(state, tileArray, _zoneController.Zones);

            // Aspect Source phase: clear and repopulate dynamic aspects
            foreach (var placed in state.PlacedPieces)
                placed.DynamicAspects.Clear();

            foreach (var placed in state.PlacedPieces)
            foreach (var config in state.AspectSources)
                config.source.Apply(placed, context);

            var pieceStates = state.PlacedPieces.Select(placed =>
            {
                var effects = _emotionRules
                    .Select(config => config.rule.Evaluate(placed, context))
                    .Where(effect => effect != null)
                    .ToList();
                return new PieceEmotionState(placed, effects);
            }).ToList();

            LastResult = new EmotionEvaluationResult(pieceStates);
            OnEvaluationChanged?.Invoke(LastResult);
        }
    }
}