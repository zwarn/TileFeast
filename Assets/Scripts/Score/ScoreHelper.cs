using System;
using System.Collections.Generic;
using System.Linq;
using Shape.model;
using UnityEngine;

namespace Score
{
    public static class ScoreHelper
    {
        
        public static ShapeSO[,] ConvertTiles(Dictionary<Vector2Int, PlacedShape> tiles, int width, int height)
        {
            var result = new ShapeSO[width, height];

            tiles.ToList().ForEach(pair =>
            {
                Vector2Int pos = pair.Key;
                if (pos.x < width && pos.y < height)
                {
                    result[pos.x, pos.y] = pair.Value.Shape;
                }
            });

            return result;
        }
        
        public static List<List<Vector2Int>> GetGroups(ShapeSO[,] tilesArray, Func<ShapeSO, bool> check)
        {
            List<List<Vector2Int>> groups = new List<List<Vector2Int>>();
            List<Vector2Int> visited = new List<Vector2Int>();

            for (int x = 0; x < tilesArray.GetLength(0); x++)
            {
                for (int y = 0; y < tilesArray.GetLength(1); y++)
                {
                    var current = new Vector2Int(x, y);
                    if (visited.Contains(current))
                    {
                        continue;
                    }

                    if (check.Invoke(tilesArray[x, y]))
                    {
                        groups.Add(FillGroup(tilesArray, check, x, y, visited));
                    }
                }
            }

            return groups;
        }

        private static List<Vector2Int> FillGroup(ShapeSO[,] tilesArray, Func<ShapeSO, bool> check, int x, int y,
            List<Vector2Int> visited)
        {
            List<Vector2Int> tiles = new List<Vector2Int>();

            var startPosition = new Vector2Int(x, y);
            tiles.Add(startPosition);
            visited.Add(startPosition);

            Queue<Vector2Int> candidates = new Queue<Vector2Int>();
            FindCandidates(tilesArray, check, visited, new Vector2Int(x, y), candidates);

            while (candidates.Count > 0)
            {
                var position = candidates.Dequeue();

                tiles.Add(position);
                visited.Add(position);

                FindCandidates(tilesArray, check, visited, position, candidates);
            }

            return tiles;
        }

        private static void FindCandidates(ShapeSO[,] tilesArray, Func<ShapeSO, bool> check, List<Vector2Int> visited,
            Vector2Int position, Queue<Vector2Int> candidates)
        {
            foreach (var candidate in Neighbors(tilesArray, position.x, position.y).Where(pos => !visited.Contains(pos))
                         .Where(pos => !candidates.Contains(pos))
                         .Where(pos => check.Invoke(tilesArray[pos.x, pos.y])))
            {
                candidates.Enqueue(candidate);
            }
        }

        private static List<Vector2Int> Neighbors(ShapeSO[,] tiles, int x, int y)
        {
            List<Vector2Int> result = new List<Vector2Int>();

            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);

            if (x > 0)
            {
                result.Add(new Vector2Int(x - 1, y));
            }

            if (y > 0)
            {
                result.Add(new Vector2Int(x, y - 1));
            }

            if (x < width - 1)
            {
                result.Add(new Vector2Int(x + 1, y));
            }

            if (y < height - 1)
            {
                result.Add(new Vector2Int(x, y + 1));
            }

            return result;
        }
        
    }
}