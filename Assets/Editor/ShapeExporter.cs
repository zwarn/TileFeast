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

            return $"  {{\n" +
                   $"    \"name\": \"{shapeName}\",\n" +
                   $"    \"pivot\": [{pivotCol}, {pivotRow}],\n" +
                   $"    \"shape\": [{string.Join(", ", rows)}]\n" +
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
