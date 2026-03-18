using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pieces;
using Pieces.Aspects;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Editor
{
    public class PiecePreviewGenerator : EditorWindow
    {
        private const string SettingsPath = "Assets/Editor/PiecePreviewGeneratorSettings.asset";
        private const string OutputFolder = "Assets/Art/Images/GeneratedPreviews";
        private const string PiecesFolder = "Assets/ScriptableObjects/Pieces";

        private PiecePreviewGeneratorSettings _settings;
        private SerializedObject _settingsSO;
        private SerializedProperty _aspectTilesetsProp;

        // Batch creation state
        private AspectSO _batchAspect;
        private string _batchSaveFolder = PiecesFolder;
        private bool _batchGeneratePreviews = true;

        [MenuItem("Tools/Piece Tool")]
        public static void Open() => GetWindow<PiecePreviewGenerator>("Piece Tool");

        private void OnEnable()
        {
            _settings = LoadOrCreateSettings();
            _settingsSO = new SerializedObject(_settings);
            _aspectTilesetsProp = _settingsSO.FindProperty("aspectTilesets");
        }

        private void OnGUI()
        {
            // ── Aspect → Tileset Mappings ────────────────────────────────────
            GUILayout.Label("Aspect → Tileset Mappings", EditorStyles.boldLabel);
            _settingsSO.Update();
            EditorGUILayout.PropertyField(_aspectTilesetsProp, true);
            _settingsSO.ApplyModifiedProperties();

            EditorGUILayout.Space();

            // ── Batch Create Pieces ──────────────────────────────────────────
            GUILayout.Label("Batch Create Pieces", EditorStyles.boldLabel);

            _batchAspect = (AspectSO)EditorGUILayout.ObjectField(
                "Aspect", _batchAspect, typeof(AspectSO), false);
            _batchSaveFolder = EditorGUILayout.TextField("Save Folder", _batchSaveFolder);
            _batchGeneratePreviews = EditorGUILayout.Toggle("Also Generate Previews", _batchGeneratePreviews);

            GUI.enabled = _batchAspect != null;
            if (GUILayout.Button("Create All Shapes for Aspect"))
                CreateAllShapesForAspect();
            GUI.enabled = true;

            EditorGUILayout.Space();

            // ── Generate Previews ────────────────────────────────────────────
            GUILayout.Label("Generate Previews", EditorStyles.boldLabel);

            if (GUILayout.Button("Generate All"))
            {
                var guids = AssetDatabase.FindAssets("t:PieceSO", new[] { PiecesFolder });
                var pieces = guids
                    .Select(g => AssetDatabase.LoadAssetAtPath<PieceSO>(AssetDatabase.GUIDToAssetPath(g)))
                    .Where(p => p != null)
                    .ToList();
                GeneratePreviews(pieces);
            }

            if (GUILayout.Button("Generate Selected"))
            {
                var pieces = Selection.GetFiltered<PieceSO>(SelectionMode.Assets).ToList();
                if (pieces.Count == 0)
                    Debug.LogWarning("PiecePreviewGenerator: No PieceSO assets selected.");
                else
                    GeneratePreviews(pieces);
            }
        }

        // ------------------------------------------------------------------ batch create

        private void CreateAllShapesForAspect()
        {
            var shapeNames = ShapeExporter.ReadShapeNames();
            if (shapeNames.Length == 0)
            {
                Debug.LogError("No shapes found in shapes.json. Run 'Tools > Export Shapes to JSON' first.");
                return;
            }

            // Use the Unity asset name (e.g. "Blue") rather than AspectSO.name field
            string aspectObjectName = ((UnityEngine.Object)_batchAspect).name;

            if (!AssetDatabase.IsValidFolder(_batchSaveFolder))
            {
                string parent = Path.GetDirectoryName(_batchSaveFolder).Replace('\\', '/');
                string child = Path.GetFileName(_batchSaveFolder);
                AssetDatabase.CreateFolder(parent, child);
            }

            var created = new List<PieceSO>();
            int newCount = 0, updatedCount = 0;

            for (int i = 0; i < shapeNames.Length; i++)
            {
                string shapeName = shapeNames[i];
                EditorUtility.DisplayProgressBar("Creating Pieces",
                    $"{aspectObjectName}-{shapeName} ({i + 1}/{shapeNames.Length})",
                    (float)i / shapeNames.Length);

                string pieceName = $"{aspectObjectName}-{shapeName}";
                string assetPath = $"{_batchSaveFolder}/{pieceName}.asset";

                var existing = AssetDatabase.LoadAssetAtPath<PieceSO>(assetPath);
                if (existing != null)
                {
                    bool changed = false;
                    if (existing.shapeName != shapeName)
                    {
                        existing.shapeName = shapeName;
                        existing.LoadShapeFromJSON();
                        changed = true;
                    }
                    if (existing.aspects == null || !existing.aspects.Contains(_batchAspect))
                    {
                        existing.aspects = new List<AspectSO> { _batchAspect };
                        changed = true;
                    }
                    if (changed) EditorUtility.SetDirty(existing);
                    created.Add(existing);
                    updatedCount++;
                }
                else
                {
                    var pso = CreateInstance<PieceSO>();
                    pso.shapeName = shapeName;
                    pso.aspects = new List<AspectSO> { _batchAspect };
                    pso.shape = new List<Vector2Int>();
                    AssetDatabase.CreateAsset(pso, assetPath);
                    pso.LoadShapeFromJSON();
                    EditorUtility.SetDirty(pso);
                    created.Add(pso);
                    newCount++;
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Batch create: {newCount} created, {updatedCount} updated for aspect '{aspectObjectName}'.");

            if (_batchGeneratePreviews && created.Count > 0)
                GeneratePreviews(created);
        }

        private void GeneratePreviews(List<PieceSO> pieces)
        {
            EnsureOutputFolder();

            int success = 0;
            for (int i = 0; i < pieces.Count; i++)
            {
                var pso = pieces[i];
                EditorUtility.DisplayProgressBar("Generating Piece Previews",
                    $"Processing {pso.name} ({i + 1}/{pieces.Count})", (float)i / pieces.Count);

                if (GeneratePreview(pso))
                    success++;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"PiecePreviewGenerator: Generated {success}/{pieces.Count} previews.");
        }

        private bool GeneratePreview(PieceSO pso)
        {
            if (pso.shape == null || pso.shape.Count == 0)
            {
                Debug.LogWarning($"PiecePreviewGenerator: '{pso.name}' has no shape, skipping.");
                return false;
            }

            TileBase tileset = ResolveTileset(pso);
            if (tileset == null)
            {
                Debug.LogWarning($"PiecePreviewGenerator: '{pso.name}' has no matching tileset, skipping.");
                return false;
            }

            EnsureRuleTileSpritesReadable(tileset);

            var tileSprites = GetTileSpritesForShape(pso.shape, tileset);

            int tilePixels = 16;
            foreach (var kvp in tileSprites)
            {
                if (kvp.Value != null)
                {
                    tilePixels = Mathf.FloorToInt(kvp.Value.rect.width);
                    break;
                }
            }

            int minX = pso.shape.Min(p => p.x);
            int maxX = pso.shape.Max(p => p.x);
            int minY = pso.shape.Min(p => p.y);
            int maxY = pso.shape.Max(p => p.y);

            // Gameplay sprite (no face — FaceView adds animated eyes at runtime)
            Texture2D gameplayTex = CompositeTexture(pso, tileSprites, tilePixels,
                minX, minY, maxX, maxY, withFace: false);
            Sprite gameplaySprite = SaveAndAssign(pso, gameplayTex, tilePixels,
                minX, minY, maxX, maxY, suffix: "", assignTo: "sprite");
            DestroyImmediate(gameplayTex);

            // UI preview sprite (with baked face — for inventory/hand display)
            Texture2D previewTex = CompositeTexture(pso, tileSprites, tilePixels,
                minX, minY, maxX, maxY, withFace: true);
            Sprite previewSprite = SaveAndAssign(pso, previewTex, tilePixels,
                minX, minY, maxX, maxY, suffix: "_preview", assignTo: "previewSprite");
            DestroyImmediate(previewTex);

            return gameplaySprite != null;
        }

        // ------------------------------------------------------------------ tile sprites

        private Dictionary<Vector2Int, Sprite> GetTileSpritesForShape(List<Vector2Int> shape, TileBase tileset)
        {
            // RuleOverrideTile delegates GetTileData to m_InstanceTile (a RuleTile with sprites baked in).
            // Neighbor matching checks tilemap.GetTile(neighbor) == m_InstanceTile, so we must set
            // m_InstanceTile in the tilemap — not the RuleOverrideTile itself.
            TileBase tileToSet;
            if (tileset is RuleOverrideTile overrideTile)
            {
                if (!overrideTile.m_InstanceTile)
                    overrideTile.Override();
                tileToSet = overrideTile.m_InstanceTile;
            }
            else
            {
                tileToSet = tileset;
            }

            var gridGO = new GameObject("_PreviewGrid") { hideFlags = HideFlags.HideAndDontSave };
            gridGO.AddComponent<Grid>();
            var tilemapGO = new GameObject("_PreviewTilemap") { hideFlags = HideFlags.HideAndDontSave };
            tilemapGO.transform.SetParent(gridGO.transform);
            var tilemap = tilemapGO.AddComponent<Tilemap>();
            tilemapGO.AddComponent<TilemapRenderer>();

            foreach (var pos in shape)
                tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), tileToSet);

            var result = new Dictionary<Vector2Int, Sprite>();
            foreach (var pos in shape)
            {
                var tileData = new TileData();
                tileset.GetTileData(new Vector3Int(pos.x, pos.y, 0), tilemap, ref tileData);
                result[pos] = tileData.sprite;
            }

            DestroyImmediate(gridGO);
            return result;
        }

        // ------------------------------------------------------------------ texture composition

        private Texture2D CompositeTexture(
            PieceSO pso,
            Dictionary<Vector2Int, Sprite> tileSprites,
            int tilePixels,
            int minX, int minY, int maxX, int maxY,
            bool withFace = true)
        {
            int width  = (maxX - minX + 1) * tilePixels;
            int height = (maxY - minY + 1) * tilePixels;

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.SetPixels(new Color[width * height]);

            // Tiles (base layer, no alpha blend)
            foreach (var pos in pso.shape)
            {
                if (!tileSprites.TryGetValue(pos, out var sprite) || sprite == null) continue;
                int px = (pos.x - minX) * tilePixels;
                int py = (pos.y - minY) * tilePixels;
                BlitSprite(tex, sprite, px, py);
            }

            if (!withFace) { tex.Apply(); return tex; }

            // Eyes
            var leftEyeSprite  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Images/Eyes/LeftEye.png");
            var rightEyeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Images/Eyes/RightEye.png");

            if (leftEyeSprite != null)
            {
                EnsureReadable(leftEyeSprite.texture);
                BlitSpriteCentered(tex, leftEyeSprite,
                    (pso.leftEyePosition.x - minX) * tilePixels + tilePixels / 2,
                    (pso.leftEyePosition.y - minY) * tilePixels + tilePixels / 2);
            }
            if (rightEyeSprite != null)
            {
                EnsureReadable(rightEyeSprite.texture);
                BlitSpriteCentered(tex, rightEyeSprite,
                    (pso.rightEyePosition.x - minX) * tilePixels + tilePixels / 2,
                    (pso.rightEyePosition.y - minY) * tilePixels + tilePixels / 2);
            }

            // Mouth
            if (pso.hasMouth)
            {
                string mouthPath = pso.mouthDouble
                    ? "Assets/Art/Images/Eyes/BigMouth.png"
                    : "Assets/Art/Images/Eyes/SmallMouth.png";
                var mouthSprite = AssetDatabase.LoadAssetAtPath<Sprite>(mouthPath);
                if (mouthSprite != null)
                {
                    EnsureReadable(mouthSprite.texture);
                    int mx = Mathf.RoundToInt((pso.mouthPosition.x - minX) * tilePixels + tilePixels * 0.5f);
                    int my = Mathf.RoundToInt((pso.mouthPosition.y - minY) * tilePixels + tilePixels * 0.5f);
                    BlitSpriteCentered(tex, mouthSprite, mx, my);
                }
            }

            tex.Apply();
            return tex;
        }

        // ------------------------------------------------------------------ save

        private Sprite SaveAndAssign(PieceSO pso, Texture2D tex, int tilePixels,
            int minX, int minY, int maxX, int maxY,
            string suffix = "", string assignTo = "sprite")
        {
            string assetPath = $"{OutputFolder}/{pso.name}{suffix}.png";
            string absPath = Path.Combine(Application.dataPath,
                assetPath.Replace("Assets/", "").Replace("/", Path.DirectorySeparatorChar.ToString()));

            File.WriteAllBytes(absPath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                float pivotNormX = (-minX + 0.5f) / (maxX - minX + 1);
                float pivotNormY = (-minY + 0.5f) / (maxY - minY + 1);

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                // spriteAlignment must be Custom for spritePivot to take effect
                var texSettings = new TextureImporterSettings();
                importer.ReadTextureSettings(texSettings);
                texSettings.spriteAlignment = (int)SpriteAlignment.Custom;
                importer.SetTextureSettings(texSettings);
                importer.spritePivot = new Vector2(pivotNormX, pivotNormY);
                importer.spritePixelsPerUnit = tilePixels;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.isReadable = false;
                importer.SaveAndReimport();
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null)
            {
                if (assignTo == "previewSprite")
                    pso.previewSprite = sprite;
                else
                    pso.sprite = sprite;
                EditorUtility.SetDirty(pso);
            }

            return sprite;
        }

        // ------------------------------------------------------------------ helpers

        private TileBase ResolveTileset(PieceSO pso)
        {
            if (pso.aspects == null || _settings == null) return null;
            foreach (var aspect in pso.aspects)
            {
                foreach (var entry in _settings.aspectTilesets)
                {
                    if (entry.aspect == aspect && entry.tileset != null)
                        return entry.tileset;
                }
            }
            return null;
        }

        private static void EnsureReadable(Texture2D tex)
        {
            if (tex == null || tex.isReadable) return;
            string path = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(path)) return;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
        }

        private static void EnsureRuleTileSpritesReadable(TileBase tileset)
        {
            if (tileset == null) return;
            var textures = new HashSet<Texture2D>();

            // For RuleOverrideTile, the final sprites live in m_InstanceTile's tiling rules
            // (Override() has already baked overrides into it). Fall back to m_Sprites if needed.
            if (tileset is RuleOverrideTile overrideTile)
            {
                var source = overrideTile.m_InstanceTile != null ? overrideTile.m_InstanceTile : overrideTile.m_Tile;
                if (source != null)
                    foreach (var rule in source.m_TilingRules)
                        foreach (var sprite in rule.m_Sprites)
                            if (sprite?.texture != null)
                                textures.Add(sprite.texture);
            }
            else if (tileset is RuleTile ruleTile)
            {
                foreach (var rule in ruleTile.m_TilingRules)
                    foreach (var sprite in rule.m_Sprites)
                        if (sprite?.texture != null)
                            textures.Add(sprite.texture);
            }

            foreach (var tex in textures)
                EnsureReadable(tex);
        }

        private static void BlitSprite(Texture2D dest, Sprite sprite, int destX, int destY)
        {
            if (sprite == null) return;
            int srcX = Mathf.FloorToInt(sprite.rect.x);
            int srcY = Mathf.FloorToInt(sprite.rect.y);
            int w = Mathf.FloorToInt(sprite.rect.width);
            int h = Mathf.FloorToInt(sprite.rect.height);

            int srcOffX = Mathf.Max(0, -destX);
            int srcOffY = Mathf.Max(0, -destY);
            int clampedW = Mathf.Min(w - srcOffX, dest.width - Mathf.Max(0, destX));
            int clampedH = Mathf.Min(h - srcOffY, dest.height - Mathf.Max(0, destY));
            if (clampedW <= 0 || clampedH <= 0) return;

            Color[] pixels = sprite.texture.GetPixels(srcX + srcOffX, srcY + srcOffY, clampedW, clampedH);
            dest.SetPixels(destX + srcOffX, destY + srcOffY, clampedW, clampedH, pixels);
        }

        private static void BlitSpriteCentered(Texture2D dest, Sprite sprite, int centerX, int centerY)
        {
            if (sprite == null) return;
            int w = Mathf.FloorToInt(sprite.rect.width);
            int h = Mathf.FloorToInt(sprite.rect.height);
            int destX = centerX - w / 2;
            int destY = centerY - h / 2;

            int srcX = Mathf.FloorToInt(sprite.rect.x);
            int srcY = Mathf.FloorToInt(sprite.rect.y);

            for (int row = 0; row < h; row++)
            {
                int dy = destY + row;
                if (dy < 0 || dy >= dest.height) continue;
                for (int col = 0; col < w; col++)
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
                            src.a + bg.a * (1f - src.a)));
                    }
                }
            }
        }

        private static void EnsureOutputFolder()
        {
            string absPath = Path.Combine(Application.dataPath,
                OutputFolder.Replace("Assets/", "").Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (!Directory.Exists(absPath))
                Directory.CreateDirectory(absPath);
        }

        private static PiecePreviewGeneratorSettings LoadOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<PiecePreviewGeneratorSettings>(SettingsPath);
            if (settings == null)
            {
                settings = CreateInstance<PiecePreviewGeneratorSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }
    }
}
