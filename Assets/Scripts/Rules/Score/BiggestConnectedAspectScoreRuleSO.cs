using System.Collections.Generic;
using System.Linq;
using Piece.aspect;
using UnityEngine;

namespace Rules.Score
{
    [CreateAssetMenu(fileName = "ScoreRule", menuName = "ScoreRule/BiggestConnectedAspect", order = 0)]
    public class BiggestConnectedAspectScoreRuleSO : ScoreRuleSO
    {
        public AspectSO aspect;
        private int _score;

        private List<Vector2Int> _scoringTiles = new();

        public override int GetScore()
        {
            return _score;
        }

        public override void CalculateScore(ScoreContext context)
        {
            var groups = RulesHelper.GetGroups(context.TileArray, so => so != null && so.aspects.Contains(aspect));
            var biggestGroup = groups.OrderByDescending(group => group.Count).FirstOrDefault();
            var count = biggestGroup?.Count ?? 0;

            _scoringTiles = biggestGroup;
            _score = count;
        }

        public override List<Vector2Int> GetScoreArea()
        {
            return _scoringTiles;
        }

        public override string GetText()
        {
            return $"Your biggest connected group of things {aspect.name}";
        }
    }
}