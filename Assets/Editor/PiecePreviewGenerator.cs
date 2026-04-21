using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pieces;
using Pieces.Aspects;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Editor
{
    public class PiecePreviewGenerator : EditorWindow
    {
        private const string SettingsPath = "Assets/Editor/PiecePreviewGeneratorSettings.asset";
        private const string OutputFolder = "Assets/Art/Images/GeneratedPreviews";
        private const string PiecesFolder = "Assets/ScriptableObjects/Pieces";
        private const string GameControllerPrefabPath = "Assets/Prefabs/GameController.prefab";

        private PiecePreviewGeneratorSettings _settings;
        private SerializedObject _settingsSO;
        private SerializedProperty _aspectTilesetsProp;

        // Batch creation state
        private AspectSO _batchAspect;
        private string _batchSaveFolder = PiecesFolder;
        private bool _batchGeneratePreviews = true;

        // Add-new-shape state
        private string _newShapeName = "";
        private string _newShapeText = "";
        private int _newPivotCol;
        private int _newPivotRow;

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

            // ── Face Sprites ─────────────────────────────────────────────────
            GUILayout.Label("Face Sprites", EditorStyles.boldLabel);
            _settingsSO.Update();
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("leftEyeSprite"), new GUIContent("Left Eye"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("rightEyeSprite"), new GUIContent("Right Eye"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("smallMouthSprite"), new GUIContent("Small Mouth"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("bigMouthSprite"), new GUIContent("Big Mouth"));
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

            // ── Add New Shape ────────────────────────────────────────────────
            GUILayout.Label("Add New Shape", EditorStyles.boldLabel);

            _newShapeName = EditorGUILayout.TextField("Shape Name", _newShapeName);

            EditorGUILayout.LabelField("Shape (rows top-to-bottom, X=filled, O=empty)");
            _newShapeText = EditorGUILayout.TextArea(_newShapeText, GUILayout.MinHeight(60));

            EditorGUILayout.BeginHorizontal();
            _newPivotCol = EditorGUILayout.IntField("Pivot Col (from left)", _newPivotCol);
            _newPivotRow = EditorGUILayout.IntField("Pivot Row (from top)", _newPivotRow);
            EditorGUILayout.EndHorizontal();

            GUI.enabled = !string.IsNullOrWhiteSpace(_newShapeName) && !string.IsNullOrWhiteSpace(_newShapeText);
            if (GUILayout.Button("Create New Shape"))
                CreateNewShape();
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

                var (pso, wasNew) = UpsertPieceAsset(_batchAspect, aspectObjectName, shapeName, _batchSaveFolder);
                created.Add(pso);
                if (wasNew) newCount++; else updatedCount++;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Batch create: {newCount} created, {updatedCount} updated for aspect '{aspectObjectName}'.");

            if (_batchGeneratePreviews && created.Count > 0)
                GeneratePreviews(created);
        }

        private (PieceSO piece, bool wasNew) UpsertPieceAsset(
            AspectSO aspect, string aspectObjectName, string shapeName, string saveFolder)
        {
            string pieceName = $"{aspectObjectName}-{shapeName}";
            string assetPath = $"{saveFolder}/{pieceName}.asset";

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
                if (existing.aspects == null || !existing.aspects.Contains(aspect))
                {
                    existing.aspects = new List<AspectSO> { aspect };
                    changed = true;
                }
                if (changed) EditorUtility.SetDirty(existing);
                return (existing, false);
            }

            var pso = CreateInstance<PieceSO>();
            pso.shapeName = shapeName;
            pso.aspects = new List<AspectSO> { aspect };
            pso.shape = new List<Vector2Int>();
            AssetDatabase.CreateAsset(pso, assetPath);
            pso.LoadShapeFromJSON();
            EditorUtility.SetDirty(pso);
            return (pso, true);
        }

        // ------------------------------------------------------------------ add new shape

        private void CreateNewShape()
        {
            string name = _newShapeName.Trim();
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("Create New Shape: name is empty.");
                return;
            }

            var rawLines = _newShapeText.Replace("\r", "").Split('\n');
            var rows = rawLines.Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim()).ToArray();

            if (rows.Length == 0)
            {
                Debug.LogError("Create New Shape: shape is empty.");
                return;
            }

            int width = rows[0].Length;
            foreach (var row in rows)
            {
                if (row.Length != width)
                {
                    Debug.LogError($"Create New Shape: rows are not all the same width (expected {width}, got '{row}' of length {row.Length}).");
                    return;
                }
                foreach (var ch in row)
                {
                    if (ch != 'X' && ch != 'O')
                    {
                        Debug.LogError($"Create New Shape: invalid character '{ch}' — use only 'X' (filled) and 'O' (empty).");
                        return;
                    }
                }
            }

            if (!rows.Any(r => r.Contains('X')))
            {
                Debug.LogError("Create New Shape: shape has no filled ('X') cells.");
                return;
            }

            if (_newPivotCol < 0 || _newPivotCol >= width || _newPivotRow < 0 || _newPivotRow >= rows.Length)
            {
                Debug.LogError($"Create New Shape: pivot ({_newPivotCol}, {_newPivotRow}) is outside shape bounds ({width} cols × {rows.Length} rows).");
                return;
            }

            var existingNames = ShapeExporter.ReadShapeNames();
            if (existingNames.Contains(name))
            {
                Debug.LogError($"Create New Shape: a shape named '{name}' already exists in shapes.json. Pick a different name.");
                return;
            }

            // ── Append to shapes.json ───────────────────────────────────────
            string entry = ShapeExporter.BuildJsonEntryFromRows(name, rows, _newPivotCol, _newPivotRow);
            ShapeExporter.AppendShapeEntry(entry);
            Debug.Log($"Create New Shape: appended '{name}' to shapes.json.");

            // ── Create a PieceSO for every mapped aspect with a tileset ─────
            if (!AssetDatabase.IsValidFolder(PiecesFolder))
            {
                string parent = Path.GetDirectoryName(PiecesFolder).Replace('\\', '/');
                string child = Path.GetFileName(PiecesFolder);
                AssetDatabase.CreateFolder(parent, child);
            }

            var created = new List<PieceSO>();
            foreach (var mapping in _settings.aspectTilesets)
            {
                if (mapping.aspect == null) continue;
                if (mapping.tileset == null)
                {
                    Debug.LogWarning($"Create New Shape: aspect '{((Object)mapping.aspect).name}' has no tileset mapped — skipping piece creation for it.");
                    continue;
                }

                string aspectObjectName = ((Object)mapping.aspect).name;
                var (pso, _) = UpsertPieceAsset(mapping.aspect, aspectObjectName, name, PiecesFolder);
                created.Add(pso);
            }

            AssetDatabase.SaveAssets();

            if (created.Count == 0)
            {
                Debug.LogWarning("Create New Shape: no aspects with tilesets are configured — JSON was updated but no PieceSOs created.");
                return;
            }

            // ── Previews ────────────────────────────────────────────────────
            GeneratePreviews(created);

            // ── Register in PieceRepository ─────────────────────────────────
            AddPiecesToRepository(created);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Create New Shape: '{name}' added — {created.Count} PieceSO(s) created, previews generated, registered in PieceRepository.");

            // Clear form on success
            _newShapeName = "";
            _newShapeText = "";
            _newPivotCol = 0;
            _newPivotRow = 0;
            GUI.FocusControl(null);
        }

        private void AddPiecesToRepository(List<PieceSO> newPieces)
        {
            int sceneUpdates = AddPiecesToSceneInstances(newPieces);
            int prefabAdded = AddPiecesToPrefab(newPieces);

            if (sceneUpdates == 0 && prefabAdded == 0)
                Debug.LogWarning("Create New Shape: no PieceRepository found in any loaded scene or on the GameController prefab — the new pieces were NOT registered. Open the scene containing PieceRepository and run again.");
        }

        private int AddPiecesToSceneInstances(List<PieceSO> newPieces)
        {
            // PieceRepository.allPieces is typically overridden on the scene instance
            // (it masks whatever the prefab has), so the scene is the authoritative target.
            var repos = Object.FindObjectsByType<Pieces.PieceRepository>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            int reposUpdated = 0;
            foreach (var repo in repos)
            {
                int added = AppendToAllPieces(repo, newPieces);
                if (added <= 0) continue;

                EditorUtility.SetDirty(repo);
                EditorSceneManager.MarkSceneDirty(repo.gameObject.scene);
                reposUpdated++;
                Debug.Log($"Create New Shape: added {added} piece(s) to PieceRepository in scene '{repo.gameObject.scene.name}'. (Save the scene to persist.)");
            }
            return reposUpdated;
        }

        private int AddPiecesToPrefab(List<PieceSO> newPieces)
        {
            if (!File.Exists(GameControllerPrefabPath))
                return 0;

            var prefabRoot = PrefabUtility.LoadPrefabContents(GameControllerPrefabPath);
            try
            {
                var repo = prefabRoot.GetComponentInChildren<Pieces.PieceRepository>(includeInactive: true);
                if (repo == null) return 0;

                int added = AppendToAllPieces(repo, newPieces);
                if (added <= 0) return 0;

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, GameControllerPrefabPath);
                Debug.Log($"Create New Shape: added {added} piece(s) to PieceRepository on {GameControllerPrefabPath}.");
                return added;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static int AppendToAllPieces(Pieces.PieceRepository repo, List<PieceSO> newPieces)
        {
            var so = new SerializedObject(repo);
            var arr = so.FindProperty("allPieces");

            var existing = new HashSet<Object>();
            for (int i = 0; i < arr.arraySize; i++)
                existing.Add(arr.GetArrayElementAtIndex(i).objectReferenceValue);

            int added = 0;
            foreach (var p in newPieces)
            {
                if (p == null || existing.Contains(p)) continue;
                int idx = arr.arraySize;
                arr.InsertArrayElementAtIndex(idx);
                arr.GetArrayElementAtIndex(idx).objectReferenceValue = p;
                existing.Add(p);
                added++;
            }

            if (added > 0) so.ApplyModifiedPropertiesWithoutUndo();
            return added;
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
            // Belt-and-suspenders: re-load shape + face data from JSON right before composing,
            // so any asset-database churn between create-and-preview can't leave these stale.
            if (!string.IsNullOrEmpty(pso.shapeName))
                pso.LoadShapeFromJSON();

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

        // ------------------------------------------------------------------ face sprite lookup

        private Sprite GetFaceSprite(string slot)
        {
            if (_settings == null) return null;
            return slot switch
            {
                "Left Eye"   => _settings.leftEyeSprite,
                "Right Eye"  => _settings.rightEyeSprite,
                "Small Mouth"=> _settings.smallMouthSprite,
                "Big Mouth"  => _settings.bigMouthSprite,
                _ => null
            };
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

            // Face sprites are assigned directly in the tool settings window.
            var leftEyeSprite  = GetFaceSprite("Left Eye");
            var rightEyeSprite = GetFaceSprite("Right Eye");

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

            if (pso.hasMouth)
            {
                var mouthSprite = GetFaceSprite(pso.mouthDouble ? "Big Mouth" : "Small Mouth");
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
            else
            {
                Debug.LogWarning($"PiecePreviewGenerator: wrote {assetPath} but failed to load it as Sprite — '{assignTo}' on '{pso.name}' will remain unchanged.");
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
