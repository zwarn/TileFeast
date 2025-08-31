using System.Collections.Generic;
using UnityEngine;

namespace Piece.model
{
    [CreateAssetMenu(fileName = "Piece", menuName = "Piece", order = 0)]
    public class PieceSO : ScriptableObject
    {
        public List<Vector2Int> tilePosition;
        public Sprite sprite;
        public List<AspectSO> aspects;
    }
}