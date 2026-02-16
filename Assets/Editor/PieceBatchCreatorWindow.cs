using System.Collections.Generic;
using Pieces;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class PieceBatchCreatorWindow : EditorWindow
    {
        private Texture2D spritesheet;
        private string saveFolder = "Assets/ScriptableObjects/Pieces";

        [MenuItem("Tools/Piece Batch Creator")]
        public static void Open()
        {
            GetWindow<PieceBatchCreatorWindow>("Piece Batch Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Batch Create Piece ScriptableObjects", EditorStyles.boldLabel);

            spritesheet = (Texture2D)EditorGUILayout.ObjectField("Spritesheet", spritesheet, typeof(Texture2D), false);
            saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);

            if (GUILayout.Button("Create Pieces"))
                CreatePiecesFromSheet();
        }

        private void CreatePiecesFromSheet()
        {
            if (spritesheet == null)
            {
                Debug.LogError("No spritesheet assigned.");
                return;
            }

            // Get all sprites sliced from this sheet
            string path = AssetDatabase.GetAssetPath(spritesheet);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            List<Sprite> sprites = new List<Sprite>();
            foreach (var asset in assets)
            {
                if (asset is Sprite s)
                    sprites.Add(s);
            }

            if (sprites.Count == 0)
            {
                Debug.LogError("No sprites found in this texture. Make sure it's sliced.");
                return;
            }

            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder(saveFolder))
            {
                Debug.Log($"Folder '{saveFolder}' does not exist. Creating it.");
                AssetDatabase.CreateFolder("Assets", saveFolder.Replace("Assets/", ""));
            }

            foreach (var sprite in sprites)
            {
                string soPath = $"{saveFolder}/{sprite.name}Piece.asset";

                PieceSO PieceSO = ScriptableObject.CreateInstance<PieceSO>();
                PieceSO.sprite = sprite; // triggers OnValidate automatically later

                AssetDatabase.CreateAsset(PieceSO, soPath);

                EditorUtility.SetDirty(PieceSO);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created {sprites.Count} new PieceSO assets in {saveFolder}");
        }
    }
}
