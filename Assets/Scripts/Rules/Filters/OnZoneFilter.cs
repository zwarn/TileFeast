using System;
using System.Collections.Generic;
using System.Linq;
using Pieces;
using UnityEngine;
using Zones;

namespace Rules.Filters
{
    public enum ZoneProximityMode
    {
        OnZone,
        AdjacentToZone
    }

    [Serializable]
    public class OnZoneFilter : PieceFilter
    {
        [UnityEngine.Tooltip("The zone type to check (matched by reference)")]
        public ZoneSO zoneType;

        [UnityEngine.Tooltip("On: piece tile lies on the zone. Adjacent: piece tile is cardinally adjacent to the zone.")]
        public ZoneProximityMode mode = ZoneProximityMode.OnZone;

        public override bool Matches(PlacedPiece piece, EmotionContext context)
        {
            if (zoneType == null || context.Zones == null) return false;
            var zones = context.Zones.Where(z => z.zoneType == zoneType).ToList();
            if (zones.Count == 0) return false;

            return mode == ZoneProximityMode.OnZone
                ? IsOnZone(piece, zones)
                : IsAdjacentToZone(piece, zones);
        }

        public override string GetDescription()
        {
            var zoneName = zoneType != null ? zoneType.name : "?";
            var modeText = mode == ZoneProximityMode.OnZone ? "on" : "adjacent to";
            return $"pieces {modeText} a {zoneName} zone";
        }

        private static bool IsOnZone(PlacedPiece piece, List<Zone> zones)
        {
            var tilePositions = piece.GetTilePosition();
            return zones.Any(z => tilePositions.Any(tp => z.positions.Contains(tp)));
        }

        private static bool IsAdjacentToZone(PlacedPiece piece, List<Zone> zones)
        {
            var tilePositions = piece.GetTilePosition();
            var zonePositions = new HashSet<Vector2Int>(zones.SelectMany(z => z.positions));
            return tilePositions.Any(tp =>
                zonePositions.Contains(tp) ||
                CardinalNeighbors(tp).Any(n => zonePositions.Contains(n)));
        }

        private static IEnumerable<Vector2Int> CardinalNeighbors(Vector2Int pos)
        {
            yield return new Vector2Int(pos.x + 1, pos.y);
            yield return new Vector2Int(pos.x - 1, pos.y);
            yield return new Vector2Int(pos.x, pos.y + 1);
            yield return new Vector2Int(pos.x, pos.y - 1);
        }
    }
}
