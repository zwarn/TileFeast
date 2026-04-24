using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

namespace BoardExpansion
{
    public static class RandomBoardExpansionFactory
    {
        public static BoardExpansion Create(
            BoardExpansionPreviewGenerator generator,
            GameController gameController,
            int tileCount = 7)
        {
            var data = GenerateData(tileCount);
            return new BoardExpansion(data, generator, gameController);
        }

        private static BoardExpansionData GenerateData(int tileCount)
        {
            var shape = new HashSet<Vector2Int> { Vector2Int.zero };
            var frontier = new List<Vector2Int>
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };

            while (shape.Count < tileCount && frontier.Count > 0)
            {
                int idx = Random.Range(0, frontier.Count);
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

            var data = new BoardExpansionData(shape.ToList());

            var hCandidates = shape
                .Where(p => shape.Contains(new Vector2Int(p.x, p.y + 1)))
                .ToList();
            if (hCandidates.Count > 0)
                data.HorizontalWalls.Add(hCandidates[Random.Range(0, hCandidates.Count)]);

            var vCandidates = shape
                .Where(p => shape.Contains(new Vector2Int(p.x + 1, p.y)))
                .ToList();
            if (vCandidates.Count > 0)
                data.VerticalWalls.Add(vCandidates[Random.Range(0, vCandidates.Count)]);

            return data;
        }
    }
}
