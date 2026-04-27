using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Placeables.ZonePlacementS
{
    public class ZonePlacementPreviewGenerator
    {
        private const int P = 32;

        public Sprite Generate(ZonePlacementData data)
        {
            if (data.Shape == null || data.Shape.Count == 0) return null;
            if (data.ZoneType?.zoneTile == null) return null;

            int minX = data.Shape.Min(p => p.x), minY = data.Shape.Min(p => p.y);
            int maxX = data.Shape.Max(p => p.x), maxY = data.Shape.Max(p => p.y);

            var w = maxX - minX + 1;
            var h = maxY - minY + 1;

            var tex = new Texture2D(w * P, h * P, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.SetPixels(new Color[w * P * h * P]);

            var sprites = GetSpritesForTiles(data.Shape, data.ZoneType.zoneTile);
            foreach (var pos in data.Shape)
            {
                if (!sprites.TryGetValue(pos, out var sprite) || sprite == null) continue;
                BlitSpriteCentered(tex, sprite,
                    (pos.x - minX) * P + P / 2,
                    (pos.y - minY) * P + P / 2);
            }

            tex.Apply();

            var pivotX = (-minX + 0.5f) / w;
            var pivotY = (-minY + 0.5f) / h;
            return Sprite.Create(tex, new Rect(0, 0, w * P, h * P), new Vector2(pivotX, pivotY), P);
        }

        private static Dictionary<Vector2Int, Sprite> GetSpritesForTiles(List<Vector2Int> positions, TileBase tile)
        {
            var gridGO = new GameObject("_ZonePlacementPreviewGrid") { hideFlags = HideFlags.HideAndDontSave };
            gridGO.AddComponent<Grid>();
            var tilemapGO = new GameObject("_ZonePlacementPreviewTilemap") { hideFlags = HideFlags.HideAndDontSave };
            tilemapGO.transform.SetParent(gridGO.transform);
            var tilemap = tilemapGO.AddComponent<Tilemap>();
            tilemapGO.AddComponent<TilemapRenderer>();

            foreach (var pos in positions)
                tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), tile);

            var result = new Dictionary<Vector2Int, Sprite>();
            foreach (var pos in positions)
            {
                var tileData = new TileData();
                tile.GetTileData(new Vector3Int(pos.x, pos.y, 0), tilemap, ref tileData);
                result[pos] = tileData.sprite;
            }

            Object.DestroyImmediate(gridGO);
            return result;
        }

        private static void BlitSpriteCentered(Texture2D dest, Sprite sprite, int centerX, int centerY)
        {
            if (sprite == null) return;
            var sw = Mathf.FloorToInt(sprite.rect.width);
            var sh = Mathf.FloorToInt(sprite.rect.height);
            var destX = centerX - sw / 2;
            var destY = centerY - sh / 2;
            var srcX = Mathf.FloorToInt(sprite.rect.x);
            var srcY = Mathf.FloorToInt(sprite.rect.y);

            for (var row = 0; row < sh; row++)
            {
                var dy = destY + row;
                if (dy < 0 || dy >= dest.height) continue;
                for (var col = 0; col < sw; col++)
                {
                    var dx = destX + col;
                    if (dx < 0 || dx >= dest.width) continue;
                    var src = sprite.texture.GetPixel(srcX + col, srcY + row);
                    if (src.a <= 0f) continue;
                    if (src.a >= 1f)
                    {
                        dest.SetPixel(dx, dy, src);
                    }
                    else
                    {
                        var bg = dest.GetPixel(dx, dy);
                        dest.SetPixel(dx, dy, new Color(
                            src.r * src.a + bg.r * (1f - src.a),
                            src.g * src.a + bg.g * (1f - src.a),
                            src.b * src.a + bg.b * (1f - src.a),
                            src.a + bg.a * (1f - src.a)));
                    }
                }
            }
        }
    }
}