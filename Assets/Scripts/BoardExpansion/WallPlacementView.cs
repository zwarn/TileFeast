using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BoardExpansion
{
    public class WallPlacementView : MonoBehaviour
    {
        [SerializeField] private Tilemap horizontalWallTilemap;
        [SerializeField] private TileBase horizontalWallTile;
        [SerializeField] private Tilemap verticalWallTilemap;
        [SerializeField] private TileBase verticalWallTile;

        private static readonly Color PreviewColor = new Color(1f, 1f, 1f, 0.6f);
        private static readonly Color InvalidTint  = new Color(1f, 0.35f, 0.35f, 0.6f);

        public void SetData(WallPlacement placement)
        {
            var c     = placement.CurrentCenter;
            var base3 = new Vector3(-c.x - 0.5f, -c.y - 0.5f, 0f);
            horizontalWallTilemap.transform.localPosition = base3 + new Vector3(0f,    -0.5f, 0f);
            verticalWallTilemap.transform.localPosition   = base3 + new Vector3(-0.5f,  0f,   0f);

            SetTiles(horizontalWallTilemap, placement.CurrentHorizontalWalls
                .Select(w => MakeTile(w.x, w.y + 1, horizontalWallTile)));

            SetTiles(verticalWallTilemap, placement.CurrentVerticalWalls
                .Select(v => MakeTile(v.x + 1, v.y, verticalWallTile)));
        }

        public void SetPreviewValid(bool valid)
        {
            var tint = valid ? Color.white : InvalidTint;
            horizontalWallTilemap.color = tint;
            verticalWallTilemap.color   = tint;
        }

        private void OnDisable()
        {
            horizontalWallTilemap.ClearAllTiles();
            verticalWallTilemap.ClearAllTiles();
        }

        private static void SetTiles(Tilemap tilemap, IEnumerable<TileChangeData> tiles)
        {
            tilemap.ClearAllTiles();
            tilemap.SetTiles(tiles.ToArray(), false);
        }

        private static TileChangeData MakeTile(int x, int y, TileBase tile)
            => new TileChangeData(new Vector3Int(x, y, 0), tile, PreviewColor, Matrix4x4.identity);
    }
}
