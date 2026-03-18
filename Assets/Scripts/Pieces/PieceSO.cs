using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pieces.Aspects;
using UnityEditor;
using UnityEngine;

namespace Pieces
{
    [CreateAssetMenu(fileName = "Piece", menuName = "Piece", order = 0)]
    public class PieceSO : ScriptableObject
    {
        public List<Vector2Int> shape;
        public string shapeName;
        public Sprite sprite;
        public Sprite previewSprite;
        public List<AspectSO> aspects;

        public Vector2Int leftEyePosition;
        public Vector2Int rightEyePosition;
        public bool       hasMouth;
        public Vector2    mouthPosition;
        public bool       mouthDouble;

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(shapeName))
                LoadShapeFromJSON();
        }

        public void LoadShapeFromJSON()
        {
            try
            {
                string path = Application.dataPath + "/Shapes/shapes.json";
                if (!File.Exists(path)) return;

                string raw = File.ReadAllText(path);
                string wrapped = "{\"shapes\":" + raw + "}";
                var library = JsonUtility.FromJson<ShapeLibrary>(wrapped);

                if (library?.shapes == null) return;

                var entry = library.shapes.FirstOrDefault(s => s.name == shapeName);
                if (entry == null)
                {
                    Debug.LogWarning($"PieceSO ‘{name}’: shape ‘{shapeName}’ not found in shapes.json");
                    return;
                }

                shape = new List<Vector2Int>();
                int pivotCol = entry.pivot[0];
                int pivotRow = entry.pivot[1];

                for (int r = 0; r < entry.shape.Length; r++)
                {
                    string row = entry.shape[r];
                    for (int c = 0; c < row.Length; c++)
                    {
                        if (row[c] == 'X')
                            shape.Add(new Vector2Int(c - pivotCol, pivotRow - r));
                    }
                }

                if (entry.leftEye?.Length == 2)
                    leftEyePosition = new Vector2Int(entry.leftEye[0], entry.leftEye[1]);
                if (entry.rightEye?.Length == 2)
                    rightEyePosition = new Vector2Int(entry.rightEye[0], entry.rightEye[1]);
                hasMouth = entry.mouth?.Length == 2;
                if (hasMouth)
                    mouthPosition = new Vector2(entry.mouth[0], entry.mouth[1]);
                mouthDouble = entry.mouthDouble;

                EditorUtility.SetDirty(this);
            }
            catch (Exception e)
            {
                Debug.LogError($"PieceSO ‘{name}’: Failed to load shape from JSON: {e.Message}");
            }
        }

        [Serializable]
        private class ShapeLibrary
        {
            public ShapeData[] shapes;
        }

        [Serializable]
        private class ShapeData
        {
            public string  name;
            public int[]   pivot;
            public string[] shape;
            public int[]   leftEye;
            public int[]   rightEye;
            public float[] mouth;
            public bool    mouthDouble;
        }

        // Deprecated: shape is now loaded from shapes.json via shapeName.
        // Kept for reference; no longer called from OnValidate.
        private void GenerateShapeFromImage()
        {
            shape.Clear();

            Texture2D tex = sprite.texture;
            Rect rect = sprite.rect;
            Vector2 pivot = sprite.pivot;
            float ppu = sprite.pixelsPerUnit;

            string path = AssetDatabase.GetAssetPath(sprite);
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);

            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            int rangeX = Mathf.CeilToInt(rect.width / ppu / 2f);
            int rangeY = Mathf.CeilToInt(rect.height / ppu / 2f);

            for (int y = -rangeY; y <= rangeY; y++)
            {
                for (int x = -rangeX; x <= rangeX; x++)
                {
                    float pixelX = rect.x + pivot.x + x * ppu;
                    float pixelY = rect.y + pivot.y + y * ppu;

                    if (pixelX < rect.x || pixelX >= rect.xMax ||
                        pixelY < rect.y || pixelY >= rect.yMax)
                        continue;

                    Color pixel = tex.GetPixel((int)pixelX, (int)pixelY);

                    if (pixel.a > 0.1f)
                        shape.Add(new Vector2Int(x, y));
                }
            }

            EditorUtility.SetDirty(this);
        }

#endif
    }
}