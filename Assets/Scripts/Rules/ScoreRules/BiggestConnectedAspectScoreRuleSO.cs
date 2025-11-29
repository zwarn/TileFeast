using System.Collections.Generic;
using System.Linq;
using Piece.Aspect;
using UnityEngine;

namespace Rules.ScoreRules
{
    [CreateAssetMenu(fileName = "ScoreRule", menuName = "ScoreRule/BiggestConnectedAspect", order = 0)]
    public class BiggestConnectedAspectScoreRuleSO : ScoreRuleSO
    {
        public AspectSO aspectSO;
        private int _score;

        private Aspect Aspect => new(aspectSO);

        private List<Vector2Int> _scoringTiles = new();

        public override int GetScore()
        {
            return _score;
        }

        public override void CalculateScore(RuleContext context)
        {
            var groups = RulesHelper.GetGroups(context.TileArray,
                piece => piece != null && piece.aspects.Contains(Aspect));
            var biggestGroup = groups.OrderByDescending(group => group.Count).FirstOrDefault();
            var count = biggestGroup?.Count ?? 0;

            _scoringTiles = biggestGroup;
            _score = count;
        }

        public override HighlightData GetScoreArea()
        {
            return new(Color.cyan, _scoringTiles.ToList());
        }

        public override string GetText()
        {
            return $"Your biggest connected group of things {aspectSO.name}";
        }
    }
}