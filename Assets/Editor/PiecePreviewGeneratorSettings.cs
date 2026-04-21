using System;
using System.Collections.Generic;
using Pieces.Aspects;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Editor
{
    public class PiecePreviewGeneratorSettings : ScriptableObject
    {
        public List<AspectTilesetEntry> aspectTilesets = new List<AspectTilesetEntry>();

        [Header("Face Sprites (drag sub-sprites from face.png)")]
        public Sprite leftEyeSprite;
        public Sprite rightEyeSprite;
        public Sprite smallMouthSprite;
        public Sprite bigMouthSprite;
    }

    [Serializable]
    public struct AspectTilesetEntry
    {
        public AspectSO aspect;
        public TileBase tileset; // accepts both RuleTile and RuleOverrideTile
    }
}
