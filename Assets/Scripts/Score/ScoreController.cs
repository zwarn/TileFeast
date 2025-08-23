using System;
using System.Collections.Generic;
using System.Linq;
using Board;
using Hand;
using Shape.model;
using UnityEngine;
using Zenject;

namespace Score
{
    public class ScoreController : MonoBehaviour
    {
        [Inject] private InteractionController _interactionController;
        [Inject] private BoardController _boardController;

        [SerializeField] private List<ScoreCondition> _scoreConditions;

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
        }

        private void OnDisable()
        {
            _interactionController.OnBoardChanged -= CalculateScore;
        }

        private void CalculateScore()
        {
            var tilesDictionary = _boardController.GetShapeByPosition();
            ShapeSO[,] tilesArray = ScoreHelper.ConvertTiles(tilesDictionary, _width, _height);

            _scoreConditions.ForEach(condition => condition.CalculateScore(tilesArray));
            int points = _scoreConditions.Select(condition => condition.GetScore()).Sum();

            Debug.Log($"Score : {points}");
        }
    }
}