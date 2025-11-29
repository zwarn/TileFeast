using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rules.ZoneRules
{
    [CreateAssetMenu(fileName = "ZoneRule", menuName = "ZoneRule/LeaveEmptyForBonus", order = 0)]
    public class LeaveEmptyForBonusRuleSO : ZoneRuleSO
    {
        private bool _isFree;
        private List<Vector2Int> _position;
        private List<Vector2Int> _covered;

        public override int GetScore()
        {
            return _isFree ? 1 : 0;
        }

        public override void Calculate(ZoneContext context)
        {
            _position = context.ZonePosition;
            _covered = _position.Where(pos => context.TileArray[pos.x, pos.y] != null).ToList();
            _isFree = _covered.Count == 0;
        }

        public override bool IsSatisfied()
        {
            return true;
        }

        public override HighlightData GetHighlight()
        {
            if (_isFree)
            {
                return new HighlightData(Color.cyan, _position);
            }

            return new HighlightData(Color.red, _covered);
        }

        public override string GetText()
        {
            return "Gives points if left uncovered";
        }
    }
}