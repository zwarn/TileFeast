using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pieces
{
    /// <summary>
    /// Result of a piece shape match, containing the piece and the rotation that matches.
    /// </summary>
    public struct PieceMatch
    {
        public PieceSO Piece;
        public int Rotation;

        public PieceMatch(PieceSO piece, int rotation)
        {
            Piece = piece;
            Rotation = rotation;
        }
    }

    public class PieceRepository : MonoBehaviour
    {
        [SerializeField] private List<PieceSO> allPieces;

        public IReadOnlyList<PieceSO> AllPieces => allPieces;

        /// <summary>
        /// Finds all pieces whose shape matches the given shape (considering all 4 rotations).
        /// </summary>
        public List<PieceMatch> FindPiecesByShape(List<Vector2Int> shape)
        {
            var matches = new List<PieceMatch>();

            if (shape == null || shape.Count == 0)
                return matches;

            foreach (var piece in allPieces)
            {
                if (piece.shape == null || piece.shape.Count == 0)
                    continue;

                var rotation = ShapeHelper.FindMatchingRotation(shape, piece.shape);
                if (rotation >= 0)
                {
                    matches.Add(new PieceMatch(piece, rotation));
                }
            }

            return matches;
        }

        /// <summary>
        /// Finds all pieces whose shape matches the given shape, filtered by a predicate.
        /// </summary>
        public List<PieceMatch> FindPiecesByShape(List<Vector2Int> shape, System.Func<PieceSO, bool> filter)
        {
            var matches = new List<PieceMatch>();

            if (shape == null || shape.Count == 0)
                return matches;

            foreach (var piece in allPieces.Where(filter))
            {
                if (piece.shape == null || piece.shape.Count == 0)
                    continue;

                var rotation = ShapeHelper.FindMatchingRotation(shape, piece.shape);
                if (rotation >= 0)
                {
                    matches.Add(new PieceMatch(piece, rotation));
                }
            }

            return matches;
        }
    }
}
