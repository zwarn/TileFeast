using UnityEngine;
using UnityEngine.Tilemaps;

namespace Placeables.BoardExpansions
{
    [CreateAssetMenu(fileName = "BoardExpansionPreviewSettings", menuName = "BoardExpansion/Preview Settings")]
    public class BoardExpansionPreviewSettings : ScriptableObject
    {
        public TileBase boardTile;
        public TileBase horizontalWallTile;
        public TileBase verticalWallTile;
    }
}