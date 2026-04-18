using System;
using Pieces;

namespace Rules.AspectSources
{
    /// <summary>
    /// Grants dynamic aspects to placed pieces during evaluation, before emotion rules run.
    /// </summary>
    [Serializable]
    public abstract class AspectSource
    {
        public abstract void Apply(PlacedPiece piece, EmotionContext context);
        public abstract string GetDescription();
    }
}
