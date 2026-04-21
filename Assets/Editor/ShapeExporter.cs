using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Pieces;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class ShapeExporter
    {
        private const string ShapesJsonPath = "Assets/Shapes/shapes.json";
        private const string PiecesFolder = "Assets/ScriptableObjects/Pieces";

        [MenuItem("Tools/Export Shapes to JSON")]
        public static void ExportShapes()
        {
            var guids = AssetDatabase.FindAssets("t:PieceSO", new[] { PiecesFolder });
            var allPieces = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<PieceSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(p => p != null)
                .ToList();

            var groups = new Dictionary<string, List<PieceSO>>();
            foreach (var pso in allPieces)
            {
                string shapeName = DeriveShapeName(pso.name);
                if (!groups.ContainsKey(shapeName))
                    groups[shapeName] = new List<PieceSO>();
                groups[shapeName].Add(pso);
            }

            var entries = new List<string>();
            foreach (var kvp in groups.OrderBy(k => k.Key))
            {
                string shapeName = kvp.Key;
                PieceSO representative = kvp.Value[0];

                if (representative.shape == null || representative.shape.Count == 0)
                {
                    Debug.LogWarning($"ShapeExporter: '{shapeName}' has empty shape, skipping JSON entry.");
                }
                else
                {
                    entries.Add(BuildJsonEntry(shapeName, representative.shape));
                }

                foreach (var pso in kvp.Value)
                {
                    pso.shapeName = shapeName;
                    EditorUtility.SetDirty(pso);
                }
            }

            if (!AssetDatabase.IsValidFolder("Assets/Shapes"))
                AssetDatabase.CreateFolder("Assets", "Shapes");

            string json = "[\n" + string.Join(",\n", entries) + "\n]";
            File.WriteAllText(Path.GetFullPath(ShapesJsonPath), json, Encoding.UTF8);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"ShapeExporter: Exported {entries.Count} shapes to {ShapesJsonPath}. Stamped shapeName on {allPieces.Count} PieceSOs.");
        }

        public static string DeriveShapeName(string assetName)
        {
            return Regex.Replace(assetName, @"^(Blue|Red|Yellow)-", "");
        }

        public static string[] ReadShapeNames()
        {
            string path = Application.dataPath + "/Shapes/shapes.json";
            if (!File.Exists(path)) return new string[0];
            try
            {
                string wrapped = "{\"shapes\":" + File.ReadAllText(path) + "}";
                var library = JsonUtility.FromJson<ShapeNamesLibrary>(wrapped);
                return library?.shapes?.Select(s => s.name).Where(n => !string.IsNullOrEmpty(n)).ToArray()
                       ?? new string[0];
            }
            catch
            {
                return new string[0];
            }
        }

        [Serializable]
        private class ShapeNamesLibrary { public ShapeNameEntry[] shapes; }

        [Serializable]
        private class ShapeNameEntry { public string name; }

        // ------------------------------------------------------------------ face data

        public struct FaceData
        {
            public Vector2Int leftEye;
            public Vector2Int rightEye;
            public bool hasMouth;
            public Vector2 mouthPosition;
            public bool mouthDouble;
        }

        public static FaceData ComputeFaceData(List<Vector2Int> shape)
        {
            int topY = shape.Max(p => p.y);
            var topRow = shape.Where(p => p.y == topY).ToList();
            var fd = new FaceData
            {
                leftEye  = new Vector2Int(topRow.Min(p => p.x), topY),
                rightEye = new Vector2Int(topRow.Max(p => p.x), topY)
            };

            foreach (int row in shape.Select(p => p.y).Distinct().OrderBy(y => y))
            {
                var xs = shape.Where(p => p.y == row).Select(p => p.x).OrderBy(x => x).ToList();
                if (xs.Last() - xs.First() + 1 != xs.Count) continue;
                float cx = (xs.First() + xs.Last()) / 2f;
                fd.hasMouth      = true;
                fd.mouthDouble   = xs.Count % 2 == 0;
                fd.mouthPosition = new Vector2(fd.mouthDouble ? cx : Mathf.RoundToInt(cx), row);
                break;
            }
            return fd;
        }

        // ------------------------------------------------------------------ json building

        /// <summary>
        /// Builds a shapes.json entry from raw rows (top-to-bottom) and an explicit pivot.
        /// Face data (eyes/mouth) is derived from the piece-space shape using the same
        /// transform PieceSO.LoadShapeFromJSON applies: (col - pivotCol, pivotRow - row).
        /// </summary>
        public static string BuildJsonEntryFromRows(string name, string[] rows, int pivotCol, int pivotRow)
        {
            var shape = new List<Vector2Int>();
            for (int r = 0; r < rows.Length; r++)
            {
                string row = rows[r];
                for (int c = 0; c < row.Length; c++)
                {
                    if (row[c] == 'X')
                        shape.Add(new Vector2Int(c - pivotCol, pivotRow - r));
                }
            }

            FaceData fd = ComputeFaceData(shape);

            var sb = new StringBuilder();
            sb.Append("  {\n");
            sb.Append($"    \"name\": \"{name}\",\n");
            sb.Append($"    \"pivot\": [{pivotCol}, {pivotRow}],\n");

            var quoted = new List<string>(rows.Length);
            foreach (var row in rows) quoted.Add($"\"{row}\"");
            sb.Append($"    \"shape\": [{string.Join(", ", quoted)}],\n");

            sb.Append($"    \"leftEye\": [{fd.leftEye.x}, {fd.leftEye.y}],\n");
            sb.Append($"    \"rightEye\": [{fd.rightEye.x}, {fd.rightEye.y}]");
            if (fd.hasMouth)
            {
                string mx = fd.mouthPosition.x == Mathf.Floor(fd.mouthPosition.x)
                    ? $"{(int)fd.mouthPosition.x}"
                    : fd.mouthPosition.x.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                sb.Append($",\n    \"mouth\": [{mx}, {(int)fd.mouthPosition.y}],");
                sb.Append($"\n    \"mouthDouble\": {(fd.mouthDouble ? "true" : "false")}");
            }
            sb.Append("\n  }");
            return sb.ToString();
        }

        /// <summary>
        /// Appends a single entry (already JSON-formatted via BuildJsonEntryFromRows) to shapes.json,
        /// preserving the existing layout, and triggers an asset reimport so ShapesJsonPostprocessor fires.
        /// </summary>
        public static void AppendShapeEntry(string entry)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Shapes"))
                AssetDatabase.CreateFolder("Assets", "Shapes");

            string path = Path.GetFullPath(ShapesJsonPath);

            if (!File.Exists(path))
            {
                File.WriteAllText(path, "[\n" + entry + "\n]", Encoding.UTF8);
            }
            else
            {
                string text = File.ReadAllText(path);
                int closeIdx = text.LastIndexOf(']');
                if (closeIdx < 0)
                {
                    File.WriteAllText(path, "[\n" + entry + "\n]", Encoding.UTF8);
                }
                else
                {
                    string head = text.Substring(0, closeIdx).TrimEnd();
                    if (head == "[" || head.Length == 0)
                        File.WriteAllText(path, "[\n" + entry + "\n]", Encoding.UTF8);
                    else
                        File.WriteAllText(path, head + ",\n" + entry + "\n]", Encoding.UTF8);
                }
            }

            AssetDatabase.ImportAsset(ShapesJsonPath, ImportAssetOptions.ForceUpdate);
        }

        private static string BuildJsonEntry(string shapeName, List<Vector2Int> shape)
        {
            int minX = shape.Min(p => p.x);
            int maxX = shape.Max(p => p.x);
            int minY = shape.Min(p => p.y);
            int maxY = shape.Max(p => p.y);

            int pivotCol = -minX;
            int pivotRow = maxY;

            var shapeSet = new HashSet<Vector2Int>(shape);
            var rows = new List<string>();
            for (int y = maxY; y >= minY; y--)
            {
                var sb = new StringBuilder();
                for (int x = minX; x <= maxX; x++)
                    sb.Append(shapeSet.Contains(new Vector2Int(x, y)) ? 'X' : 'O');
                rows.Add($"\"{sb}\"");
            }

            FaceData fd = ComputeFaceData(shape);

            string faceFields = $"    \"leftEye\": [{fd.leftEye.x}, {fd.leftEye.y}],\n" +
                                $"    \"rightEye\": [{fd.rightEye.x}, {fd.rightEye.y}]";
            if (fd.hasMouth)
            {
                string mx = fd.mouthPosition.x == Mathf.Floor(fd.mouthPosition.x)
                    ? $"{(int)fd.mouthPosition.x}"
                    : fd.mouthPosition.x.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                faceFields += $",\n    \"mouth\": [{mx}, {(int)fd.mouthPosition.y}]," +
                              $"\n    \"mouthDouble\": {(fd.mouthDouble ? "true" : "false")}";
            }

            return $"  {{\n" +
                   $"    \"name\": \"{shapeName}\",\n" +
                   $"    \"pivot\": [{pivotCol}, {pivotRow}],\n" +
                   $"    \"shape\": [{string.Join(", ", rows)}],\n" +
                   $"{faceFields}\n" +
                   $"  }}";
        }
    }

    public class ShapesJsonPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            bool jsonChanged = importedAssets.Any(p => p == "Assets/Shapes/shapes.json")
                            || deletedAssets.Any(p => p == "Assets/Shapes/shapes.json");
            if (!jsonChanged) return;

            var guids = AssetDatabase.FindAssets("t:PieceSO", new[] { "Assets/ScriptableObjects/Pieces" });
            int updated = 0;

            foreach (var guid in guids)
            {
                var pso = AssetDatabase.LoadAssetAtPath<PieceSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (pso != null && !string.IsNullOrEmpty(pso.shapeName))
                {
                    pso.LoadShapeFromJSON();
                    updated++;
                }
            }

            if (updated > 0)
                Debug.Log($"ShapesJsonPostprocessor: Reloaded {updated} PieceSOs from shapes.json.");
        }
    }
}
