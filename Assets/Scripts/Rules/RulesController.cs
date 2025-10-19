using System;
using System.Collections.Generic;
using System.Linq;
using Board;
using Core;
using Rules.Placement;
using Rules.Score;
using UnityEngine;
using Zenject;

namespace Rules
{
    public class RulesController : MonoBehaviour
    {
        [Inject] private BoardController _boardController;
        [Inject] private GameController _gameController;

        private List<ScoreRuleSO> _scoreRules;
        private List<PlacementRuleSO> _placementRules;

        private Vector2Int _gridSize;

        public Action<List<ScoreRuleSO>> OnScoreRuleReset;
        public Action<List<PlacementRuleSO>> OnPlacementRuleReset;

        private void OnEnable()
        {
            _gameController.OnBoardChanged += CalculateRules;
        }

        private void OnDisable()
        {
            _gameController.OnBoardChanged -= CalculateRules;
        }

        public void UpdateState(GameState gameState)
        {
            _scoreRules = gameState.ScoreRules;
            _placementRules = gameState.PlacementRules;
            _gridSize = gameState.GridSize;
            CalculateRules();
            ScoreRuleResetEvent(_scoreRules);
            PlacementRuleResetEvent(_placementRules);
        }

        public bool SatisfiesRules()
        {
            return _placementRules.All(rule => rule.IsSatisfied());
        }

        public int TotalScore()
        {
            var total = _scoreRules.Sum(rule => rule.GetScore());
            return total;
        }

        private void CalculateRules()
        {
            CalculateScoreRules();
            CalculatePlacementRules();
        }

        private void CalculateScoreRules()
        {
            var tilesDictionary = _boardController.GetPieceByPosition();
            var tileArray = RulesHelper.ConvertTiles(tilesDictionary, _gridSize.x, _gridSize.y);

            var context = new ScoreContext(_gameController.CurrentState, tileArray);

            _scoreRules.ForEach(rule => rule.CalculateScore(context));
        }

        private void CalculatePlacementRules()
        {
            var tilesDictionary = _boardController.GetPieceByPosition();
            var tileArray = RulesHelper.ConvertTiles(tilesDictionary, _gridSize.x, _gridSize.y);

            var context = new PlacementRuleContext(_gameController.CurrentState, tileArray);

            _placementRules.ForEach(rule => rule.Calculate(context));
        }

        private void ScoreRuleResetEvent(List<ScoreRuleSO> scoreRules)
        {
            OnScoreRuleReset?.Invoke(scoreRules);
        }

        private void PlacementRuleResetEvent(List<PlacementRuleSO> placementRules)
        {
            OnPlacementRuleReset?.Invoke(placementRules);
        }
    }
}