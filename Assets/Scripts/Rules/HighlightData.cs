using System.Collections.Generic;
using UnityEngine;

namespace Rules
{
    public class HighlightData
    {
        public Color Color;
        public List<Vector2Int> Positions;

        public HighlightData(Color color, List<Vector2Int> positions)
        {
            Color = color;
            Positions = positions;
        }

        public static HighlightData Empty()
        {
            return new HighlightData(Color.black, new());
        }
    }
}