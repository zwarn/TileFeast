using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using Pieces.Aspects;
using Rules;
using UnityEngine;

namespace Zones.Rules
{
    [Serializable]
    public class OnlyAspectAllowedRule : ZoneRule
    {
        public AspectSO AllowedAspect;

        private List<Vector2Int> _position;
        private List<Vector2Int> _offendingTiles;

        public override int GetScore()
        {
            return 0;
        }

        public override void Calculate(ZoneContext context)
        {
            _position = context.ZonePosition;
            _offendingTiles = _position
                .Where(pos =>
                {
                    var placedTile = context.TileArray[pos.x, pos.y];
                    return placedTile != null && !placedTile.aspects.Contains(new Aspect(AllowedAspect));
                }).ToList();
        }

        public override bool IsSatisfied()
        {
            return _offendingTiles.IsEmpty();
        }

        public override HighlightData GetHighlight()
        {
            if (_offendingTiles.IsEmpty())
            {
                return new HighlightData(Color.cyan, _position);
            }

            return new HighlightData(Color.red, _offendingTiles);
        }

        public override string GetText()
        {
            return $"Can only be covered by {AllowedAspect.name} Tiles. Can be left empty.";
        }

        public override ZoneRule Clone()
        {
            return new OnlyAspectAllowedRule { AllowedAspect = AllowedAspect };
        }
    }
}