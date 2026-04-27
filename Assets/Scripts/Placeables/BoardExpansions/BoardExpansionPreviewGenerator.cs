using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Placeables.BoardExpansions
{
    public class BoardExpansionPreviewGenerator
    {
        private const int P = 32; // pixels per tile

        private readonly BoardExpansionPreviewSettings _settings;

        public BoardExpansionPreviewGenerator(BoardExpansionPreviewSettings settings)
        {
            _settings = settings;
        }

        public Sprite Generate(BoardExpansionData data)
        {
            if (data.Shape == null || data.Shape.Count == 0) return null;

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;
            foreach (var pos in data.Shape)
            {
                if (pos.x < minX) minX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y > maxY) maxY = pos.y;
            }

            var w = maxX - minX + 1;
            var h = maxY - minY + 1;
            var tex = new Texture2D(w * P, h * P, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.SetPixels(new Color[w * P * h * P]);

            // Layer 1: board tiles
            if (_settings.boardTile != null)
            {
                var sprites = GetSpritesForTiles(data.Shape, _settings.boardTile);
                foreach (var pos in data.Shape)
                {
                    if (!sprites.TryGetValue(pos, out var sprite) || sprite == null) continue;
                    BlitSprite(tex, sprite, (pos.x - minX) * P, (pos.y - minY) * P);
                }
            }

            // Layer 2: zones (alpha blend on top of board tiles)
            foreach (var zone in data.Zones)
            {
                if (zone?.zoneType?.zoneTile == null) continue;
                var sprites = GetSpritesForTiles(zone.positions, zone.zoneType.zoneTile);
                foreach (var pos in zone.positions)
                {
                    if (!sprites.TryGetValue(pos, out var sprite) || sprite == null) continue;
                    BlitSpriteCentered(tex, sprite,
                        (pos.x - minX) * P + P / 2,
                        (pos.y - minY) * P + P / 2);
                }
            }

            // Layer 3: horizontal walls — stored as (x,y) = top edge of tile y at column x.
            // Placed at (x, y+1) in temp tilemap to match WallView convention for neighbor matching.
            if (data.HorizontalWalls.Count > 0 && _settings.horizontalWallTile != null)
            {
                var shifted = data.HorizontalWalls.Select(v => new Vector2Int(v.x, v.y + 1)).ToList();
                var sprites = GetSpritesForTiles(shifted, _settings.horizontalWallTile);
                foreach (var wall in data.HorizontalWalls)
                {
                    var key = new Vector2Int(wall.x, wall.y + 1);
                    if (!sprites.TryGetValue(key, out var sprite) || sprite == null) continue;
                    BlitSpriteCentered(tex, sprite,
                        (wall.x - minX) * P + P / 2,
                        (wall.y - minY + 1) * P);
                }
            }

            // Layer 4: vertical walls — stored as (x,y) = right edge of tile x at row y.
            // Placed at (x+1, y) in temp tilemap to match WallView convention.
            if (data.VerticalWalls.Count > 0 && _settings.verticalWallTile != null)
            {
                var shifted = data.VerticalWalls.Select(v => new Vector2Int(v.x + 1, v.y)).ToList();
                var sprites = GetSpritesForTiles(shifted, _settings.verticalWallTile);
                foreach (var wall in data.VerticalWalls)
                {
                    var key = new Vector2Int(wall.x + 1, wall.y);
                    if (!sprites.TryGetValue(key, out var sprite) || sprite == null) continue;
                    BlitSpriteCentered(tex, sprite,
                        (wall.x - minX + 1) * P,
                        (wall.y - minY) * P + P / 2);
                }
            }

            tex.Apply();

            var pivotX = (-minX + 0.5f) / w;
            var pivotY = (-minY + 0.5f) / h;
            return Sprite.Create(tex, new Rect(0, 0, w * P, h * P), new Vector2(pivotX, pivotY), P);
        }

        // Creates a temp tilemap, places all positions, queries GetTileData for each.
        // Tile textures must have Read/Write enabled in their import settings.
        private static Dictionary<Vector2Int, Sprite> GetSpritesForTiles(List<Vector2Int> positions, TileBase tile)
        {
            var gridGO = new GameObject("_ExpansionPreviewGrid") { hideFlags = HideFlags.HideAndDontSave };
            gridGO.AddComponent<Grid>();
            var tilemapGO = new GameObject("_ExpansionPreviewTilemap") { hideFlags = HideFlags.HideAndDontSave };
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

        private static void BlitSprite(Texture2D dest, Sprite sprite, int destX, int destY)
        {
            if (sprite == null) return;
            var srcX = Mathf.FloorToInt(sprite.rect.x);
            var srcY = Mathf.FloorToInt(sprite.rect.y);
            var sw = Mathf.FloorToInt(sprite.rect.width);
            var sh = Mathf.FloorToInt(sprite.rect.height);

            var offX = Mathf.Max(0, -destX);
            var offY = Mathf.Max(0, -destY);
            var clampedW = Mathf.Min(sw - offX, dest.width - Mathf.Max(0, destX));
            var clampedH = Mathf.Min(sh - offY, dest.height - Mathf.Max(0, destY));
            if (clampedW <= 0 || clampedH <= 0) return;

            var pixels = sprite.texture.GetPixels(srcX + offX, srcY + offY, clampedW, clampedH);
            dest.SetPixels(destX + offX, destY + offY, clampedW, clampedH, pixels);
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