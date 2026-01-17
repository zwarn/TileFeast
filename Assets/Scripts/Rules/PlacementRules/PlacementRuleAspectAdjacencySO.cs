using System.Collections.Generic;
using System.Linq;
using ModestTree;
using Piece.Aspect;
using UnityEngine;

namespace Rules.PlacementRules
{
    [CreateAssetMenu(menuName = "PlacementRule/ColorAdjacency", fileName = "ColorAdjacencyPlacementRuleSO", order = 0)]
    public class PlacementRuleColorAdjacency : PlacementRuleSO
    {
        public AspectSO applyTo;
        public AspectSO forbidAdjacency;

        private readonly List<Vector2Int> _offendingTiles = new();

        public override bool IsSatisfied()
        {
            return _offendingTiles.IsEmpty();
        }

        public override void Calculate(RuleContext context)
        {
            _offendingTiles.Clear();
            context.State.PlacedPieces.ForEach(piece =>
            {
                if (piece.Piece.aspects.Contains(new Aspect(applyTo)))
                {
                    var neighborPieces = RulesHelper.GetNeighborPieces(piece, context.TileArray);
                    if (neighborPieces.Any(neighborPiece =>
                            neighborPiece.aspects.Contains(new Aspect(forbidAdjacency))))
                    {
                        _offendingTiles.AddRange(piece.GetTilePosition());
                    }
                }
            });
        }

        public override HighlightData GetViolationSpots()
        {
            return new HighlightData(Color.red, _offendingTiles);
        }

        public override string GetText()
        {
            return $"{applyTo.name} Tiles are not allowed adjacent to {forbidAdjacency.name} Tiles";
        }
    }
}