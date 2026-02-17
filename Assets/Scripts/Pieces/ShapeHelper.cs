using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pieces
{
    public static class ShapeHelper
    {
        /// <summary>
        /// Normalizes a shape by translating all positions so that min x = 0 and min y = 0.
        /// </summary>
        public static List<Vector2Int> Normalize(List<Vector2Int> shape)
        {
            if (shape == null || shape.Count == 0)
                return new List<Vector2Int>();

            var minX = shape.Min(p => p.x);
            var minY = shape.Min(p => p.y);

            return shape.Select(p => new Vector2Int(p.x - minX, p.y - minY)).ToList();
        }

        /// <summary>
        /// Rotates a shape by the given number of 90-degree steps counter-clockwise.
        /// </summary>
        public static List<Vector2Int> Rotate(List<Vector2Int> shape, int rotation)
        {
            rotation = ((rotation % 4) + 4) % 4; // Normalize to 0-3

            return shape.Select(pos => rotation switch
            {
                0 => pos,
                1 => new Vector2Int(-pos.y, pos.x),
                2 => new Vector2Int(-pos.x, -pos.y),
                _ => new Vector2Int(pos.y, -pos.x)
            }).ToList();
        }

        /// <summary>
        /// Compares two shapes for equality (order-independent).
        /// Both shapes should be normalized before comparison.
        /// </summary>
        public static bool AreShapesEqual(List<Vector2Int> shapeA, List<Vector2Int> shapeB)
        {
            if (shapeA.Count != shapeB.Count)
                return false;

            var setA = new HashSet<Vector2Int>(shapeA);
            var setB = new HashSet<Vector2Int>(shapeB);

            return setA.SetEquals(setB);
        }

        /// <summary>
        /// Returns all 4 normalized rotations of a shape.
        /// </summary>
        public static List<List<Vector2Int>> GetAllNormalizedRotations(List<Vector2Int> shape)
        {
            var rotations = new List<List<Vector2Int>>();

            for (int r = 0; r < 4; r++)
            {
                var rotated = Rotate(shape, r);
                var normalized = Normalize(rotated);
                rotations.Add(normalized);
            }

            return rotations;
        }

        /// <summary>
        /// Checks if two shapes match, considering all 4 rotations of shapeB.
        /// Returns the rotation (0-3) that matches, or -1 if no match.
        /// </summary>
        public static int FindMatchingRotation(List<Vector2Int> targetShape, List<Vector2Int> pieceShape)
        {
            var normalizedTarget = Normalize(targetShape);

            for (int r = 0; r < 4; r++)
            {
                var rotated = Rotate(pieceShape, r);
                var normalizedRotated = Normalize(rotated);

                if (AreShapesEqual(normalizedTarget, normalizedRotated))
                {
                    return r;
                }
            }

            return -1;
        }
    }
}
