using System.Collections.Generic;
using UnityEngine;

namespace Piece
{
    [CreateAssetMenu(fileName = "Shape", menuName = "Shape", order = 0)]
    public class ShapeSO : ScriptableObject
    {
        public List<Vector2Int> tilePosition;
    }
}