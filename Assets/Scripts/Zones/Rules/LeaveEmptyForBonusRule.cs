using System;
using System.Collections.Generic;
using System.Linq;
using Rules;
using UnityEngine;

namespace Zones.Rules
{
    [Serializable]
    public class LeaveEmptyForBonusRule : ZoneRule
    {
        public int Points = 1;
        
        private bool _isFree;
        private List<Vector2Int> _position;
        private List<Vector2Int> _covered;

        public override int GetScore()
        {
            return _isFree ? Points : 0;
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

        public override ZoneRule Clone()
        {
            return new LeaveEmptyForBonusRule { Points = Points };
        }
    }
}