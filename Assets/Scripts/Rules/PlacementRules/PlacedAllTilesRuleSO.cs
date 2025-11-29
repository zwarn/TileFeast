using UnityEngine;

namespace Rules.PlacementRules
{
    [CreateAssetMenu(menuName = "PlacementRule/PlacedAllTiles", fileName = "PlacedAllTilesRuleSO", order = 0)]
    class PlacedAllTilesRuleSO : PlacementRuleSO
    {
        private bool _satisfied;

        public override bool IsSatisfied()
        {
            return _satisfied;
        }

        public override void Calculate(RuleContext context)
        {
            var state = context.State;
            _satisfied = state.PieceInHand == null && state.AvailablePieces.Count == 0;
        }

        public override HighlightData GetViolationSpots()
        {
            return HighlightData.Empty();
        }

        public override string GetText()
        {
            return $"Place all your tiles onto the board";
        }
    }
}