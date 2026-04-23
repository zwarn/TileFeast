using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BoardExpansion
{
    [CreateAssetMenu(fileName = "BoardExpansion", menuName = "BoardExpansion")]
    public class BoardExpansionSO : ScriptableObject
    {
        public List<Vector2Int> shape;
        public Sprite previewSprite;
        public TileBase previewTile;
        public string displayName;
    }
}
