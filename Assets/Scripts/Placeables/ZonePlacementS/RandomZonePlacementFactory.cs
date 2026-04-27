using System.Collections.Generic;
using Core;
using UnityEngine;
using Zones;

namespace Placeables.ZonePlacementS
{
    public static class RandomZonePlacementFactory
    {
        public static ZonePlacement Create(
            ZonePlacementPreviewGenerator generator,
            GameController gameController,
            ZoneSO zoneType,
            int maxTiles = 4)
        {
            var data = GenerateData(zoneType, maxTiles);
            return new ZonePlacement(data, generator, gameController);
        }

        private static ZonePlacementData GenerateData(ZoneSO zoneType, int maxTiles)
        {
            var tileCount = Random.Range(2, maxTiles + 1);

            var shape = new HashSet<Vector2Int> { Vector2Int.zero };
            var frontier = new List<Vector2Int>
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };

            while (shape.Count < tileCount && frontier.Count > 0)
            {
                var idx = Random.Range(0, frontier.Count);
                var tile = frontier[idx];
                frontier.RemoveAt(idx);

                if (shape.Contains(tile)) continue;
                shape.Add(tile);

                foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                {
                    var neighbor = tile + dir;
                    if (!shape.Contains(neighbor) && !frontier.Contains(neighbor))
                        frontier.Add(neighbor);
                }
            }

            return new ZonePlacementData(zoneType, new List<Vector2Int>(shape));
        }
    }
}