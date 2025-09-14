using System;
using System.Collections.Generic;
using System.Linq;
using Board;
using Core;
using UnityEngine;
using Zenject;

namespace Score
{
    public class ScoreController : MonoBehaviour
    {
        [Inject] private BoardController _boardController;
        [Inject] private GameController _gameController;
        private int _height;

        private List<ScoreRuleSO> _scoreRules;

        private int _width;

        public Action<List<ScoreRuleSO>> OnScoreRuleReset;

        private void Start()
        {
            _width = _boardController.width;
            _height = _boardController.height;
        }

        private void OnEnable()
        {
            _gameController.OnBoardChanged += CalculateScore;
        }

        private void OnDisable()
        {
            _gameController.OnBoardChanged -= CalculateScore;
        }

        public void UpdateState(GameState gameState)
        {
            _scoreRules = gameState.ScoreRules;
            CalculateScore();
            ScoreRuleResetEvent(_scoreRules);
        }

        public List<ScoreRuleSO> GetScoreRules()
        {
            return _scoreRules.ToList();
        }

        public int TotalScore()
        {
            var total = _scoreRules.Sum(rule => rule.GetScore());
            return total;
        }

        private void CalculateScore()
        {
            var tilesDictionary = _boardController.GetPieceByPosition();
            var tileArray = ScoreHelper.ConvertTiles(tilesDictionary, _width, _height);

            var context = new ScoreContext(_gameController.CurrentState, tileArray);

            _scoreRules.ForEach(rule => rule.CalculateScore(context));
        }

        private void ScoreRuleResetEvent(List<ScoreRuleSO> scoreRules)
        {
            OnScoreRuleReset?.Invoke(scoreRules);
        }
    }
}