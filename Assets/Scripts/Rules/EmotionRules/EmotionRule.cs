using System;
using Pieces;

namespace Rules.EmotionRules
{
    [Serializable]
    public abstract class EmotionRule
    {
        public abstract EmotionEffect Evaluate(PlacedPiece piece, EmotionContext context);
        public abstract string GetDescription();
    }
}
