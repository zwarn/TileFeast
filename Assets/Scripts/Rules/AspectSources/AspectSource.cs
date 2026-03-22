using System;
using Pieces;

namespace Rules.AspectSources
{
    [Serializable]
    public abstract class AspectSource
    {
        public abstract void Apply(PlacedPiece piece, EmotionContext context);
    }
}
