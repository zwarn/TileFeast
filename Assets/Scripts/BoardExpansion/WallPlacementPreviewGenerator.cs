using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BoardExpansion
{
    public class WallPlacementPreviewGenerator
    {
        private const int P = 32;

        private readonly BoardExpansionPreviewSettings _settings;

        public WallPlacementPreviewGenerator(BoardExpansionPreviewSettings settings)
        {
            _settings = settings;
        }

        public Sprite Generate(WallPlacementData data)
        {
            var all = data.HorizontalWalls.Concat(data.VerticalWalls).ToList();
            if (all.Count == 0) return null;

            int minX = all.Min(p => p.x), minY = all.Min(p => p.y);
            int maxX = all.Max(p => p.x), maxY = all.Max(p => p.y);

            // One extra tile in each dimension so walls at the max edges fit (H walls display
            // at y+1, V walls display at x+1 relative to their stored position).
            int w = maxX - minX + 2;
            int h = maxY - minY + 2;

            var tex = new Texture2D(w * P, h * P, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.SetPixels(new Color[w * P * h * P]);

            // Horizontal walls — stored as (x,y) = top edge of tile y. Display at (x, y+1).
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

            // Vertical walls — stored as (x,y) = right edge of tile x. Display at (x+1, y).
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

            // Pivot at the centre of the normalised bounding box (matches CurrentCenter logic).
            int maxNormX = maxX - minX;
            int maxNormY = maxY - minY;
            float pivotX = (maxNormX / 2 + 0.5f) / w;
            float pivotY = (maxNormY / 2 + 0.5f) / h;
            return Sprite.Create(tex, new Rect(0, 0, w * P, h * P), new Vector2(pivotX, pivotY), P);
        }

        private static Dictionary<Vector2Int, Sprite> GetSpritesForTiles(List<Vector2Int> positions, TileBase tile)
        {
            var gridGO = new GameObject("_WallPlacementPreviewGrid") { hideFlags = HideFlags.HideAndDontSave };
            gridGO.AddComponent<Grid>();
            var tilemapGO = new GameObject("_WallPlacementPreviewTilemap") { hideFlags = HideFlags.HideAndDontSave };
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
            int sw    = Mathf.FloorToInt(sprite.rect.width);
            int sh    = Mathf.FloorToInt(sprite.rect.height);
            int destX = centerX - sw / 2;
            int destY = centerY - sh / 2;
            int srcX  = Mathf.FloorToInt(sprite.rect.x);
            int srcY  = Mathf.FloorToInt(sprite.rect.y);

            for (int row = 0; row < sh; row++)
            {
                int dy = destY + row;
                if (dy < 0 || dy >= dest.height) continue;
                for (int col = 0; col < sw; col++)
                {
                    int dx = destX + col;
                    if (dx < 0 || dx >= dest.width) continue;
                    Color src = sprite.texture.GetPixel(srcX + col, srcY + row);
                    if (src.a <= 0f) continue;
                    if (src.a >= 1f)
                    {
                        dest.SetPixel(dx, dy, src);
                    }
                    else
                    {
                        Color bg = dest.GetPixel(dx, dy);
                        dest.SetPixel(dx, dy, new Color(
                            src.r * src.a + bg.r * (1f - src.a),
                            src.g * src.a + bg.g * (1f - src.a),
                            src.b * src.a + bg.b * (1f - src.a),
                            src.a + bg.a  * (1f - src.a)));
                    }
                }
            }
        }
    }
}
