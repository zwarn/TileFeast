using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Editor
{
    public class SpriteSheetCopierWindow : EditorWindow
    {
        Texture2D originalTexture;
        Texture2D targetTexture;
        string fromName = "Red";
        string toName = "Blue";

        [MenuItem("Tools/Sprite Sheet Copier")]
        public static void OpenWindow()
        {
            GetWindow<SpriteSheetCopierWindow>("Sprite Sheet Copier");
        }

        void OnGUI()
        {
            GUILayout.Label("Copy Names and Pivots Between Spritesheets", EditorStyles.boldLabel);

            originalTexture =
                (Texture2D)EditorGUILayout.ObjectField("Original Spritesheet", originalTexture, typeof(Texture2D),
                    false);
            targetTexture =
                (Texture2D)EditorGUILayout.ObjectField("Target Spritesheet", targetTexture, typeof(Texture2D), false);

            EditorGUILayout.Space();
            fromName = EditorGUILayout.TextField("Replace Word", fromName);
            toName = EditorGUILayout.TextField("With", toName);

            EditorGUILayout.Space();
            if (GUILayout.Button("Copy Naming and Pivots"))
            {
                if (originalTexture == null || targetTexture == null)
                {
                    Debug.LogError("You must assign both textures.");
                    return;
                }

                ApplyCopy();
            }
        }

        void ApplyCopy()
        {
            string origPath = AssetDatabase.GetAssetPath(originalTexture);
            string targetPath = AssetDatabase.GetAssetPath(targetTexture);

            var factory = new SpriteDataProviderFactories();
            factory.Init();

            TextureImporter origImporter = AssetImporter.GetAtPath(origPath) as TextureImporter;
            TextureImporter targetImporter = AssetImporter.GetAtPath(targetPath) as TextureImporter;

            if (origImporter == null || targetImporter == null)
            {
                Debug.LogError("Could not get TextureImporters.");
                return;
            }

            var origData = factory.GetSpriteEditorDataProviderFromObject(origImporter);
            var targetData = factory.GetSpriteEditorDataProviderFromObject(targetImporter);

            origData.InitSpriteEditorDataProvider();
            targetData.InitSpriteEditorDataProvider();

            var origSprites = origData.GetSpriteRects();
            var targetSprites = targetData.GetSpriteRects();

            if (origSprites.Length != targetSprites.Length)
            {
                Debug.LogError("The spritesheets do not have the same sprite count. They must be identical layout.");
                return;
            }

            for (int i = 0; i < origSprites.Length; i++)
            {
                targetSprites[i].name = origSprites[i].name.Replace(fromName, toName);
                targetSprites[i].alignment = origSprites[i].alignment;
                targetSprites[i].pivot = origSprites[i].pivot;
            }

            targetData.SetSpriteRects(targetSprites);
            targetData.Apply();

            targetImporter.SaveAndReimport();

            Debug.Log("Successfully copied pivots and names.");
        }
    }
}