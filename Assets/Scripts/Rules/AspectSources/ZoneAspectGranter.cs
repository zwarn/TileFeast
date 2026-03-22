using System;
using System.Collections.Generic;
using System.Linq;
using Pieces;
using Pieces.Aspects;
using UnityEngine;
using Zones;

namespace Rules.AspectSources
{
    [Serializable]
    public class ZoneAspectGranter : AspectSource
    {
        public ZoneSO targetZone;
        public AspectSO grantedAspect;
        public bool includeAdjacent;

        public override void Apply(PlacedPiece piece, EmotionContext context)
        {
            var targetZones = context.Zones.Where(z => z.zoneType == targetZone).ToList();

            bool matches = includeAdjacent
                ? IsOnZoneOrAdjacent(piece, targetZones)
                : IsOnZone(piece, targetZones);

            if (matches)
                piece.DynamicAspects.Add(new Aspect(grantedAspect));
        }

        private static bool IsOnZone(PlacedPiece piece, List<Zone> zones)
        {
            var tilePositions = piece.GetTilePosition();
            return zones.Any(z => tilePositions.Any(tp => z.positions.Contains(tp)));
        }

        private static bool IsOnZoneOrAdjacent(PlacedPiece piece, List<Zone> zones)
        {
            var tilePositions = piece.GetTilePosition();
            var zonePositions = new HashSet<Vector2Int>(zones.SelectMany(z => z.positions));
            return tilePositions.Any(tp =>
                zonePositions.Contains(tp) ||
                GetCardinalNeighbors(tp).Any(n => zonePositions.Contains(n)));
        }

        private static IEnumerable<Vector2Int> GetCardinalNeighbors(Vector2Int pos)
        {
            yield return new Vector2Int(pos.x + 1, pos.y);
            yield return new Vector2Int(pos.x - 1, pos.y);
            yield return new Vector2Int(pos.x, pos.y + 1);
            yield return new Vector2Int(pos.x, pos.y - 1);
        }
    }
}
