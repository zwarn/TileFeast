using System.Collections.Generic;
using System.Linq;
using Shape.model;
using UnityEngine;

namespace Score
{
    [CreateAssetMenu(fileName = "ScoreCondition", menuName = "ScoreCondition/BiggestConnectedAspect", order = 0)]
    public class BiggestConnectedAspectScoreCondition : ScoreCondition
    {
        public Aspect aspect;

        private List<Vector2Int> _scoringTiles = new();
        private int _score;

        public override int GetScore()
        {
            return _score;
        }

        public override void CalculateScore(ShapeSO[,] tiles)
        {
            var groups = ScoreHelper.GetGroups(tiles, so => so != null && so.aspects.Contains(aspect));
            var biggestGroup = groups.OrderByDescending(group => group.Count).FirstOrDefault();
            var count = biggestGroup?.Count ?? 0;

            _scoringTiles = biggestGroup;
            _score = count;
        }
    }
}