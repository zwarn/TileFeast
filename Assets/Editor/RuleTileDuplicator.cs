using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace Editor
{
    public class RuleTileDuplicator : EditorWindow
    {
        private RuleTile _source;
        private Dictionary<string, Sprite> _sourceSprites = new();
        private Dictionary<string, Sprite> _targetSprites = new();


        private List<Sprite> _dragAndDropSprites;

        private bool _dragAndDropActive
        {
            get
            {
                return _dragAndDropSprites != null
                       && _dragAndDropSprites.Count > 0;
            }
        }


        [MenuItem("zwarn/RuleTileDuplicator")]
        public static void ShowWindow()
        {
            GetWindow<RuleTileDuplicator>();
        }

        void OnGUI()
        {
            GUILayout.Label("Select Sprites to Generate Rule Tile:");
            EditorGUI.BeginChangeCheck();

            _source = (RuleTile)EditorGUILayout.ObjectField("Source Rule", _source, typeof(RuleTile), true, null);

            Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * 3);
            HandleDragAndDrop(rect);
            EditorGUI.DrawRect(rect, Color.white);

            if (EditorGUI.EndChangeCheck())
            {
                UpdateSource();
                Repaint();
            }

            GUI.enabled = IsGenerateActive();
            if (GUILayout.Button("Generate Override Rule Tile"))
            {
                GenerateRuleTile();
            }

            GUI.enabled = true;
        }

        private bool IsGenerateActive()
        {
            return _sourceSprites.Count > 0 && _targetSprites.Count > 0;
        }

        private void UpdateSource()
        {
            if (_source != null)
            {
                _sourceSprites = SpritesByFileName(_source.m_TilingRules.Select(rule => rule.m_Sprites[0]).ToList());
            }

            UpdateTransfer();
        }

        private void HandleDragAndDrop(Rect rect)
        {
            if (DragAndDrop.objectReferences.Length == 0 || !rect.Contains(Event.current.mousePosition))
            {
                return;
            }

            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                {
                    _dragAndDropSprites = GetValidSingleSprites(DragAndDrop.objectReferences);
                    if (_dragAndDropActive)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        Event.current.Use();
                        GUI.changed = true;
                    }

                    break;
                }

                case EventType.DragPerform:
                {
                    if (!_dragAndDropActive)
                        return;

                    UpdateTargetSprites(_dragAndDropSprites);

                    DragAndDropClear();
                    GUI.changed = true;
                    GUIUtility.ExitGUI();
                    break;
                }
            }

            if (Event.current.type == EventType.DragExited ||
                Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                DragAndDropClear();
            }
        }

        private void UpdateTargetSprites(List<Sprite> sprites)
        {
            _targetSprites = SpritesByFileName(sprites);

            UpdateTransfer();
        }

        private void UpdateTransfer()
        {
        }

        private static List<Sprite> GetSpritesFromTexture(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            List<Sprite> sprites = new List<Sprite>();

            foreach (Object asset in assets)
            {
                if (asset is Sprite)
                {
                    sprites.Add(asset as Sprite);
                }
            }

            return sprites;
        }

        private List<Sprite> GetValidSingleSprites(Object[] objects)
        {
            List<Sprite> result = new List<Sprite>();
            foreach (Object obj in objects)
            {
                if (obj is Sprite sprite)
                {
                    result.Add(sprite);
                }
                else if (obj is Texture2D texture2D)
                {
                    List<Sprite> sprites = GetSpritesFromTexture(texture2D);
                    if (sprites.Count > 0)
                    {
                        result.AddRange(sprites);
                    }
                }
            }

            return result;
        }

        private void DragAndDropClear()
        {
            _dragAndDropSprites = null;
            DragAndDrop.visualMode = DragAndDropVisualMode.None;
            Event.current.Use();
        }


        private void GenerateRuleTile()
        {
            if (IsGenerateActive())
            {
                var overrideTile = CreateInstance<RuleOverrideTile>();

                var overrides = new List<RuleOverrideTile.TileSpritePair>();

                foreach (var keyValuePair in _sourceSprites)
                {
                    if (_targetSprites.TryGetValue(keyValuePair.Key, out var sprite))
                    {
                        var over = new RuleOverrideTile.TileSpritePair();
                        over.m_OriginalSprite = keyValuePair.Value;
                        over.m_OverrideSprite = sprite;

                        overrides.Add(over);
                    }
                }

                overrideTile.m_Tile = _source;
                overrideTile.m_Sprites = overrides;

                var originalPath = AssetDatabase.GetAssetPath(_source);
                string directory = Path.GetDirectoryName(originalPath);
                string fileName = Path.GetFileNameWithoutExtension(originalPath);
                string extension = Path.GetExtension(originalPath);
                string newFileName = fileName + " Copy" + extension;
                string newPath = Path.Combine(directory, newFileName);

                AssetDatabase.CreateAsset(overrideTile, newPath);
                AssetDatabase.Refresh();
            }
        }

        private Dictionary<string, Sprite> SpritesByFileName(List<Sprite> sprites)
        {
            return sprites.ToDictionary(sprite => sprite.name.Split("_")[^1], sprite => sprite);
        }
    }
}