using System;
using Pieces;

namespace Rules.Checks
{
    /// <summary>
    /// Evaluates whether a condition holds for a given piece in the current context.
    /// GetDescription returns a when-clause phrase, e.g. "having the Star aspect"
    /// or "next to 2+ Red pieces".
    /// </summary>
    [Serializable]
    public abstract class EmotionCheck
    {
        public abstract CheckResult Evaluate(PlacedPiece piece, EmotionContext context);
        public abstract string GetDescription();
    }
}
