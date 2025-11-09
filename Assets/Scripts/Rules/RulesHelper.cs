using System;
using System.Collections.Generic;
using System.Linq;
using Piece;
using UnityEngine;

namespace Rules
{
    public static class RulesHelper
    {
        public static List<Piece.Piece> GetNeighborPieces(PlacedPiece piece, Piece.Piece[,] tilesArray)
        {
            return piece.GetTilePosition().SelectMany(pos => GetNeighborTiles(pos)).Distinct()
                .Where(pos => InBounds(pos, tilesArray)).Where(pos => !piece.GetTilePosition().Contains(pos))
                .Where(pos => tilesArray[pos.x, pos.y] != null)
                .Select(pos => tilesArray[pos.x, pos.y]).Distinct().ToList();
        }

        private static bool InBounds(Vector2Int pos, Piece.Piece[,] tilesArray)
        {
            return pos.x >= 0 && pos.x < tilesArray.GetLength(0)
                              && pos.y >= 0 && pos.y < tilesArray.GetLength(1);
        }

        private static List<Vector2Int> GetNeighborTiles(Vector2Int pos)
        {
            return new List<Vector2Int>
            {
                new Vector2Int(pos.x + 1, pos.y),
                new Vector2Int(pos.x - 1, pos.y),
                new Vector2Int(pos.x, pos.y + 1),
                new Vector2Int(pos.x, pos.y - 1),
            };
        }

        public static Piece.Piece[,] ConvertTiles(Dictionary<Vector2Int, PlacedPiece> tiles, int width, int height)
        {
            var result = new Piece.Piece[width, height];

            tiles.ToList().ForEach(pair =>
            {
                var pos = pair.Key;
                if (pos.x < width && pos.y < height) result[pos.x, pos.y] = pair.Value.Piece;
            });

            return result;
        }

        public static List<List<Vector2Int>> GetGroups(Piece.Piece[,] tilesArray, Func<Piece.Piece, bool> check)
        {
            var groups = new List<List<Vector2Int>>();
            var visited = new List<Vector2Int>();

            for (var x = 0; x < tilesArray.GetLength(0); x++)
            for (var y = 0; y < tilesArray.GetLength(1); y++)
            {
                var current = new Vector2Int(x, y);
                if (visited.Contains(current)) continue;

                var currentTile = tilesArray[x, y];
                if (check.Invoke(currentTile)) groups.Add(FloodGroup(tilesArray, check, x, y, visited));
            }

            return groups;
        }

        private static List<Vector2Int> FloodGroup(Piece.Piece[,] tilesArray, Func<Piece.Piece, bool> check, int x,
            int y,
            List<Vector2Int> visited)
        {
            var tiles = new List<Vector2Int>();

            var startPosition = new Vector2Int(x, y);
            tiles.Add(startPosition);
            visited.Add(startPosition);

            var candidates = new Queue<Vector2Int>();
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

        private static void FindCandidates(Piece.Piece[,] tilesArray, Func<Piece.Piece, bool> check,
            List<Vector2Int> visited,
            Vector2Int position, Queue<Vector2Int> candidates)
        {
            foreach (var candidate in Neighbors(tilesArray, position.x, position.y).Where(pos => !visited.Contains(pos))
                         .Where(pos => !candidates.Contains(pos))
                         .Where(pos => check.Invoke(tilesArray[pos.x, pos.y])))
                candidates.Enqueue(candidate);
        }

        private static List<Vector2Int> Neighbors(Piece.Piece[,] tiles, int x, int y)
        {
            var result = new List<Vector2Int>();

            var width = tiles.GetLength(0);
            var height = tiles.GetLength(1);

            if (x > 0) result.Add(new Vector2Int(x - 1, y));

            if (y > 0) result.Add(new Vector2Int(x, y - 1));

            if (x < width - 1) result.Add(new Vector2Int(x + 1, y));

            if (y < height - 1) result.Add(new Vector2Int(x, y + 1));

            return result;
        }
    }
}