using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

namespace BoardExpansion
{
    public static class RandomWallPlacementFactory
    {
        private const int Radius = 5;

        public static WallPlacement Create(
            WallPlacementPreviewGenerator generator,
            GameController gameController,
            int wallCount = 4)
        {
            var data = GenerateData(wallCount);
            return new WallPlacement(data, generator, gameController);
        }

        private static WallPlacementData GenerateData(int wallCount)
        {
            // Build a connected virtual tile region within Radius using frontier expansion.
            // Walls are only generated on internal edges between two tiles in this region,
            // which mirrors how RandomBoardExpansionFactory derives its wall candidates.
            int tileTarget = wallCount * 4 + 4;

            var tiles = new HashSet<Vector2Int> { Vector2Int.zero };
            var frontier = new List<Vector2Int>();
            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                if (WithinRadius(dir)) frontier.Add(dir);
            }

            while (tiles.Count < tileTarget && frontier.Count > 0)
            {
                int idx = Random.Range(0, frontier.Count);
                var tile = frontier[idx];
                frontier.RemoveAt(idx);

                if (tiles.Contains(tile)) continue;
                tiles.Add(tile);

                foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                {
                    var neighbor = tile + dir;
                    if (!tiles.Contains(neighbor) && !frontier.Contains(neighbor) && WithinRadius(neighbor))
                        frontier.Add(neighbor);
                }
            }

            // H candidates: positions where both (x,y) and (x,y+1) are in the tile set.
            var hCandidates = tiles
                .Where(p => tiles.Contains(new Vector2Int(p.x, p.y + 1)))
                .ToList();

            // V candidates: positions where both (x,y) and (x+1,y) are in the tile set.
            var vCandidates = tiles
                .Where(p => tiles.Contains(new Vector2Int(p.x + 1, p.y)))
                .ToList();

            // Merge into a single pool tagged by type, then draw wallCount at random.
            var pool = hCandidates.Select(p => (pos: p, isH: true))
                .Concat(vCandidates.Select(p => (pos: p, isH: false)))
                .ToList();

            var data = new WallPlacementData();
            for (int i = 0; i < wallCount && pool.Count > 0; i++)
            {
                int pick = Random.Range(0, pool.Count);
                var (pos, isH) = pool[pick];
                pool.RemoveAt(pick);

                if (isH) data.HorizontalWalls.Add(pos);
                else      data.VerticalWalls.Add(pos);
            }

            // Safety: guarantee at least one wall even if the tile region was tiny.
            if (data.HorizontalWalls.Count == 0 && data.VerticalWalls.Count == 0)
                data.HorizontalWalls.Add(Vector2Int.zero);

            return data;
        }

        private static bool WithinRadius(Vector2Int p)
            => Mathf.Abs(p.x) <= Radius && Mathf.Abs(p.y) <= Radius;
    }
}
