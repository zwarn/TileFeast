using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Returns additional board positions that are contextually relevant to this check
        /// (e.g., neighbor pieces, group members). Used for hover highlighting in the UI.
        /// </summary>
        public virtual List<HighlightGroup> GetContextHighlight(
            IEnumerable<PlacedPiece> filteredPieces, EmotionContext context)
            => new();
    }
}
