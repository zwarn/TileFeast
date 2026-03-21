using System.Collections.Generic;
using System.Linq;
using Pieces;
using Pieces.Aspects;
using UnityEngine;
using Zones;

namespace Rules.EmotionRules
{
    public enum ZoneProximityMode { OnZone, AdjacentToZone }

    [CreateAssetMenu(menuName = "EmotionRule/ZoneProximity", fileName = "ZoneProximityEmotionRule")]
    public class ZoneProximityEmotionRuleSO : EmotionRuleSO
    {
        public override EmotionEffect Evaluate(PlacedPiece piece, EmotionContext context, EmotionRuleArgs args)
        {
            var a = (ZoneProximityArgs)args;

            if (a.applyToAspect != null && !piece.Piece.aspects.Contains(new Aspect(a.applyToAspect)))
                return null;

            var targetZones = context.Zones.Where(z => z.zoneType == a.targetZoneType).ToList();

            bool condition = a.mode == ZoneProximityMode.OnZone
                ? IsOnZone(piece, targetZones)
                : IsAdjacentToZone(piece, targetZones);

            if (condition)
            {
                var modeText = a.mode == ZoneProximityMode.OnZone ? "on" : "adjacent to";
                return new EmotionEffect(a.emotionWhenTrue,
                    $"Placed {modeText} a {a.targetZoneType.name} zone", this);
            }

            if (a.emotionWhenFalse == PieceEmotion.Neutral)
                return null;

            var notModeText = a.mode == ZoneProximityMode.OnZone ? "on" : "adjacent to";
            return new EmotionEffect(a.emotionWhenFalse,
                $"Not {notModeText} a {a.targetZoneType.name} zone", this);
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
            return tilePositions.Any(tp => GetCardinalNeighbors(tp).Any(n => zonePositions.Contains(n)));
        }

        private static IEnumerable<Vector2Int> GetCardinalNeighbors(Vector2Int pos)
        {
            yield return new Vector2Int(pos.x + 1, pos.y);
            yield return new Vector2Int(pos.x - 1, pos.y);
            yield return new Vector2Int(pos.x, pos.y + 1);
            yield return new Vector2Int(pos.x, pos.y - 1);
        }

        public override string GetDescription(EmotionRuleArgs args)
        {
            var a = (ZoneProximityArgs)args;
            var target = a.applyToAspect != null ? $"{a.applyToAspect.name} pieces" : "Pieces";
            var modeText = a.mode == ZoneProximityMode.OnZone ? "on" : "adjacent to";
            return $"{target} are {a.emotionWhenTrue} when {modeText} a {a.targetZoneType.name} zone";
        }
    }
}
