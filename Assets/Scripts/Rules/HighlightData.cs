using System.Collections.Generic;
using UnityEngine;

namespace Rules
{
    public class HighlightGroup
    {
        public Color Color;
        public List<Vector2Int> Positions;

        public HighlightGroup(Color color, List<Vector2Int> positions)
        {
            Color = color;
            Positions = positions;
        }
    }

    public class HighlightData
    {
        public List<HighlightGroup> Groups;

        // Backward-compatible single-color constructor (used by ShapeTool)
        public HighlightData(Color color, List<Vector2Int> positions)
        {
            Groups = new List<HighlightGroup> { new(color, positions) };
        }

        public HighlightData(List<HighlightGroup> groups)
        {
            Groups = groups;
        }

        public static HighlightData Empty()
        {
            return new HighlightData(new List<HighlightGroup>());
        }
    }
}
