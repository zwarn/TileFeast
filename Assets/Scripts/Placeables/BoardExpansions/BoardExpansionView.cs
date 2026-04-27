using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Placeables.BoardExpansions
{
    public class BoardExpansionView : MonoBehaviour
    {
        private static readonly Color PreviewColor = new(1f, 1f, 1f, 0.6f);
        private static readonly Color InvalidTint = new(1f, 0.35f, 0.35f, 0.6f);
        [SerializeField] private Tilemap previewTilemap;
        [SerializeField] private TileBase defaultTile;

        [Header("Walls")] [SerializeField] private Tilemap horizontalWallTilemap;

        [SerializeField] private TileBase horizontalWallTile;
        [SerializeField] private Tilemap verticalWallTilemap;
        [SerializeField] private TileBase verticalWallTile;

        [Header("Zones")] [SerializeField] private Tilemap zoneTilemap;

        private void OnDisable()
        {
            previewTilemap.ClearAllTiles();
            horizontalWallTilemap.ClearAllTiles();
            verticalWallTilemap.ClearAllTiles();
            zoneTilemap.ClearAllTiles();
        }

        public void SetData(BoardExpansion expansion)
        {
            // Offset all tilemaps so the bounding-box center tile sits on the parent origin
            // (the cursor world position). The -0.5 term shifts from tile corner to tile center.
            var c = expansion.CurrentCenter;
            var base3 = new Vector3(-c.x - 0.5f, -c.y - 0.5f, 0f);
            previewTilemap.transform.localPosition = base3;
            zoneTilemap.transform.localPosition = base3;
            horizontalWallTilemap.transform.localPosition = base3 + new Vector3(0f, -0.5f, 0f);
            verticalWallTilemap.transform.localPosition = base3 + new Vector3(-0.5f, 0f, 0f);

            SetTiles(previewTilemap, expansion.CurrentShape
                .Select(p => MakeTile(p.x, p.y, defaultTile)));

            SetTiles(horizontalWallTilemap, expansion.CurrentHorizontalWalls
                .Select(w => MakeTile(w.x, w.y + 1, horizontalWallTile)));

            SetTiles(verticalWallTilemap, expansion.CurrentVerticalWalls
                .Select(v => MakeTile(v.x + 1, v.y, verticalWallTile)));

            zoneTilemap.ClearAllTiles();
            foreach (var zone in expansion.CurrentZones)
            {
                if (zone?.zoneType?.zoneTile == null) continue;
                var tiles = zone.positions
                    .Select(p => MakeTile(p.x, p.y, zone.zoneType.zoneTile))
                    .ToArray();
                zoneTilemap.SetTiles(tiles, false);
            }
        }

        public void SetPreviewValid(bool valid)
        {
            var tint = valid ? Color.white : InvalidTint;
            previewTilemap.color = tint;
            horizontalWallTilemap.color = tint;
            verticalWallTilemap.color = tint;
            zoneTilemap.color = tint;
        }

        private static void SetTiles(Tilemap tilemap, IEnumerable<TileChangeData> tiles)
        {
            tilemap.ClearAllTiles();
            tilemap.SetTiles(tiles.ToArray(), false);
        }

        private static TileChangeData MakeTile(int x, int y, TileBase tile)
        {
            return new TileChangeData(new Vector3Int(x, y, 0), tile, PreviewColor, Matrix4x4.identity);
        }
    }
}