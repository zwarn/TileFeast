using System.Collections.Generic;
using Piece.aspect;
using UnityEngine;

namespace Piece
{
    [CreateAssetMenu(fileName = "Piece", menuName = "Piece", order = 0)]
    public class PieceSO : ScriptableObject
    {
        public ShapeSO shape;
        public Sprite sprite;
        public List<AspectSO> aspects;
    }
}