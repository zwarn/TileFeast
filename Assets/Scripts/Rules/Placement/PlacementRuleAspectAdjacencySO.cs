using System.Collections.Generic;
using System.Linq;
using ModestTree;
using Piece.aspect;
using UnityEngine;

namespace Rules.Placement
{
    [CreateAssetMenu(menuName = "PlacementRule/ColorAdjacency", fileName = "ColorAdjacencyPlacementRuleSO", order = 0)]
    class PlacementRuleColorAdjacency : PlacementRuleSO
    {
        public AspectSO applyTo;
        public AspectSO forbidAdjacency;

        private readonly List<Vector2Int> _offendingTiles = new();

        public override bool IsSatisfied()
        {
            return _offendingTiles.IsEmpty();
        }

        public override void Calculate(PlacementRuleContext ruleContext)
        {
            _offendingTiles.Clear();
            ruleContext.State.PlacedPieces.ForEach(piece =>
            {
                if (piece.Piece.aspects.Contains(new Aspect(applyTo)))
                {
                    var neighborPieces = RulesHelper.GetNeighborPieces(piece, ruleContext.TileArray);
                    if (neighborPieces.Any(neighborPiece =>
                            neighborPiece.aspects.Contains(new Aspect(forbidAdjacency))))
                    {
                        _offendingTiles.AddRange(piece.GetTilePosition());
                    }
                }
            });
        }

        public override List<Vector2Int> GetViolationSpots()
        {
            return _offendingTiles;
        }

        public override string GetText()
        {
            return $"{applyTo.name} Tiles are not allowed adjacent to {forbidAdjacency.name} Tiles";
        }
    }
}