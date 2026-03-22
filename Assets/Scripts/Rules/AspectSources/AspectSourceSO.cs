using Pieces;
using UnityEngine;

namespace Rules.AspectSources
{
    public abstract class AspectSourceSO : ScriptableObject
    {
        public abstract void Apply(PlacedPiece piece, EmotionContext context, AspectSourceArgs args);
    }
}
