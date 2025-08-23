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

            var groups = ScoreHelper.GetGroups(tilesArray, so => so != null);
            var biggestGroup = groups.OrderByDescending(group => group.Count).FirstOrDefault();
            var count = biggestGroup?.Count ?? 0;
            Debug.Log($"Score : {count}");
        }
    }
}