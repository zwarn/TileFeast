using System.Collections.Generic;
using UnityEngine;

namespace Rules.Placement
{
    [CreateAssetMenu(menuName = "PlacementRule/PlacedAllTiles", fileName = "PlacedAllTilesRuleSO", order = 0)]
    class PlacedAllTilesRuleSO : PlacementRuleSO
    {
        private bool _satisfied;

        public override bool IsSatisfied()
        {
            return _satisfied;
        }

        public override void Calculate(PlacementRuleContext ruleContext)
        {
            var state = ruleContext.State;
            _satisfied = state.PieceInHand == null && state.AvailablePieces.Count == 0;
        }

        public override List<Vector2Int> GetViolationSpots()
        {
            return new List<Vector2Int>();
        }

        public override string GetText()
        {
            return $"Place all your tiles onto the board";
        }
    }
}