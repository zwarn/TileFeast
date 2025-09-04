using System;
using System.Collections.Generic;
using System.Linq;
using Piece.model;
using UnityEngine;

namespace Score
{
    [CreateAssetMenu(fileName = "ScoreRule", menuName = "ScoreRule/BiggestConnectedAspect", order = 0)]
    public class BiggestConnectedAspectScoreRule : ScoreRule
    {
        public AspectSO aspect;

        private List<Vector2Int> _scoringTiles = new();
        private int _score;

        public override int GetScore()
        {
            return _score;
        }

        public override void CalculateScore(PieceSO[,] tiles)
        {
            var groups = ScoreHelper.GetGroups(tiles, so => so != null && so.aspects.Contains(aspect));
            var biggestGroup = groups.OrderByDescending(group => group.Count).FirstOrDefault();
            var count = biggestGroup?.Count ?? 0;

            _scoringTiles = biggestGroup;
            _score = count;
        }

        public override string GetText()
        {
            return $"Your biggest connected group of things {aspect.name}";
        }
    }
}