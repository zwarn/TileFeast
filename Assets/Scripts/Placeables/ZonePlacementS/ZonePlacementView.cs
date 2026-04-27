using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Placeables.ZonePlacementS
{
    public class ZonePlacementView : MonoBehaviour
    {
        private static readonly Color PreviewColor = new(1f, 1f, 1f, 0.6f);
        private static readonly Color InvalidTint = new(1f, 0.35f, 0.35f, 0.6f);
        [SerializeField] private Tilemap zoneTilemap;

        private void OnDisable()
        {
            zoneTilemap.ClearAllTiles();
        }

        public void SetData(ZonePlacement placement)
        {
            var c = placement.CurrentCenter;
            var base3 = new Vector3(-c.x - 0.5f, -c.y - 0.5f, 0f);
            zoneTilemap.transform.localPosition = base3;

            var zoneTile = placement.ZoneType?.zoneTile;
            SetTiles(zoneTilemap, placement.CurrentShape
                .Select(p => MakeTile(p.x, p.y, zoneTile)));
        }

        public void SetPreviewValid(bool valid)
        {
            zoneTilemap.color = valid ? Color.white : InvalidTint;
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