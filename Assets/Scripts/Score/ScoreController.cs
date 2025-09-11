using System;
using System.Collections.Generic;
using System.Linq;
using Board;
using Hand;
using Piece.model;
using State;
using UnityEngine;
using Zenject;

namespace Score
{
    public class ScoreController : MonoBehaviour
    {
        [Inject] private InteractionController _interactionController;
        [Inject] private BoardController _boardController;
        [Inject] private GameStateController _gameStateController;

        public Action<List<ScoreRule>> OnScoreRuleReset;

        private List<ScoreRule> _scoreRules;

        private int _width;
        private int _height;

        private void Start()
        {
            _width = _boardController.width;
            _height = _boardController.height;
        }

        private void OnEnable()
        {
            _interactionController.OnBoardChanged += CalculateScore;
            _gameStateController.OnStateOverride += OnStateOverride;
        }

        private void OnDisable()
        {
            _interactionController.OnBoardChanged -= CalculateScore;
            _gameStateController.OnStateOverride -= OnStateOverride;
        }

        public List<ScoreRule> GetScoreRules()
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
            PieceSO[,] tilesArray = ScoreHelper.ConvertTiles(tilesDictionary, _width, _height);

            _scoreRules.ForEach(rule => rule.CalculateScore(tilesArray));
        }

        private void OnStateOverride(GameState gameState)
        {
            _scoreRules = gameState.ScoreRules;
            CalculateScore();
            ScoreRuleResetEvent(_scoreRules);
        }

        private void ScoreRuleResetEvent(List<ScoreRule> scoreRules)
        {
            OnScoreRuleReset?.Invoke(scoreRules);
        }
    }
}