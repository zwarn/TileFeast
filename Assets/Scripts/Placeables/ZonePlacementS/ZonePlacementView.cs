using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Placeables.ZonePlacementS
{
    public class ZonePlacementView : MonoBehaviour, IPlaceableView
    {
        private ZonePlacement _placeable;

        public void Bind(IPlaceable placeable) => _placeable = (ZonePlacement)placeable;

        public void Activate()
        {
            transform.localPosition = Vector3.zero;
            SetData(_placeable);
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            transform.localPosition = Vector3.zero;
            gameObject.SetActive(false);
        }

        public void UpdateFrame(Grid grid, Vector3 mouseWorldPos, Vector2Int boardCell)
        {
            var cellWorld = grid.CellToWorld(new Vector3Int(boardCell.x, boardCell.y, 0));
            transform.localPosition = new Vector3(
                cellWorld.x + 1 - mouseWorldPos.x,
                cellWorld.y + 1 - mouseWorldPos.y,
                transform.localPosition.z);
            SetPreviewValid(_placeable.IsValidPlacement(boardCell));
        }

        public void OnRotated() => SetData(_placeable);

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