using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BoardExpansion
{
    public class BoardExpansionView : MonoBehaviour
    {
        [SerializeField] private Tilemap previewTilemap;

        public void SetShape(List<Vector2Int> shapeOffsets, TileBase tile)
        {
            previewTilemap.ClearAllTiles();
            previewTilemap.SetTiles(
                shapeOffsets.Select(offset =>
                    new TileChangeData(
                        new Vector3Int(offset.x, offset.y, 0),
                        tile,
                        new Color(1f, 1f, 1f, 0.6f),
                        Matrix4x4.identity))
                .ToArray(), false);
        }

        private void OnDisable()
        {
            previewTilemap.ClearAllTiles();
        }
    }
}
