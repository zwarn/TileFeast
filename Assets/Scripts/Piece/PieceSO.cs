using System;
using System.Collections.Generic;
using Piece.aspect;
using UnityEditor;
using UnityEngine;

namespace Piece
{
    [CreateAssetMenu(fileName = "Piece", menuName = "Piece", order = 0)]
    public class PieceSO : ScriptableObject
    {
        public List<Vector2Int> shape;
        public Sprite sprite;
        public List<AspectSO> aspects;
        
#if UNITY_EDITOR

        private void OnValidate()
        {
            if (sprite != null)
            {
                GenerateShapeFromImage();
            }
        }

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
            
            // Compute tile search range (± half the sprite area in tiles)
            int rangeX = Mathf.CeilToInt(rect.width / ppu / 2f);
            int rangeY = Mathf.CeilToInt(rect.height / ppu / 2f);

            // We treat pivot as (0,0) tile coordinate
            for (int y = -rangeY; y <= rangeY; y++)
            {
                for (int x = -rangeX; x <= rangeX; x++)
                {
                    // Pixel coordinate of this tile’s center
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