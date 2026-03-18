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
    }

    [Serializable]
    public struct AspectTilesetEntry
    {
        public AspectSO aspect;
        public TileBase tileset; // accepts both RuleTile and RuleOverrideTile
    }
}
