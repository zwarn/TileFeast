using UnityEngine;
using UnityEngine.Tilemaps;

namespace BoardExpansion
{
    [CreateAssetMenu(fileName = "BoardExpansionPreviewSettings", menuName = "BoardExpansion/Preview Settings")]
    public class BoardExpansionPreviewSettings : ScriptableObject
    {
        public TileBase boardTile;
        public TileBase horizontalWallTile;
        public TileBase verticalWallTile;
    }
}
