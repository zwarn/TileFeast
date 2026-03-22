using System;
using System.Collections.Generic;
using System.Linq;
using Pieces;
using Pieces.Aspects;
using UnityEngine;
using Zones;

namespace Rules.EmotionRules
{
    public enum ZoneProximityMode { OnZone, AdjacentToZone }

    [Serializable]
    public class ZoneProximityEmotionRule : EmotionRule
    {
        [UnityEngine.Tooltip("Leave null to apply to all pieces")]
        public AspectSO applyToAspect;

        [UnityEngine.Tooltip("The zone type to check (matched by reference)")]
        public ZoneSO targetZoneType;

        public ZoneProximityMode mode = ZoneProximityMode.OnZone;

        [UnityEngine.Tooltip("Emotion when the proximity condition is true")]
        public PieceEmotion emotionWhenTrue = PieceEmotion.Happy;

        [UnityEngine.Tooltip("Emotion when the proximity condition is false. Neutral returns null (no effect).")]
        public PieceEmotion emotionWhenFalse = PieceEmotion.Neutral;

        public override EmotionEffect Evaluate(PlacedPiece piece, EmotionContext context)
        {
            if (applyToAspect != null && !piece.AllAspects.Contains(new Aspect(applyToAspect)))
                return null;

            var targetZones = context.Zones.Where(z => z.zoneType == targetZoneType).ToList();

            bool condition = mode == ZoneProximityMode.OnZone
                ? IsOnZone(piece, targetZones)
                : IsAdjacentToZone(piece, targetZones);

            if (condition)
            {
                var modeText = mode == ZoneProximityMode.OnZone ? "on" : "adjacent to";
                return new EmotionEffect(emotionWhenTrue,
                    $"Placed {modeText} a {targetZoneType.name} zone", this);
            }

            if (emotionWhenFalse == PieceEmotion.Neutral)
                return null;

            var notModeText = mode == ZoneProximityMode.OnZone ? "on" : "adjacent to";
            return new EmotionEffect(emotionWhenFalse,
                $"Not {notModeText} a {targetZoneType.name} zone", this);
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

        public override string GetDescription()
        {
            var target = applyToAspect != null ? $"{applyToAspect.name} pieces" : "Pieces";
            var modeText = mode == ZoneProximityMode.OnZone ? "on" : "adjacent to";
            return $"{target} are {emotionWhenTrue} when {modeText} a {targetZoneType.name} zone";
        }
    }
}
