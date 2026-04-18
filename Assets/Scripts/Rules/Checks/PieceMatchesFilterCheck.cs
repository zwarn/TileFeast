using System;
using Pieces;
using Rules.Filters;

namespace Rules.Checks
{
    /// <summary>
    /// Passes when the piece itself matches the supplied filter. Any PieceFilter can be
    /// re-used here, so "has aspect X", "on a Zone Y", "touching an edge" all compose
    /// without new check classes.
    /// </summary>
    [Serializable]
    public class PieceMatchesFilterCheck : EmotionCheck
    {
        [UnityEngine.SerializeReference]
        [UnityEngine.Tooltip("Filter evaluated against the piece being considered")]
        public PieceFilter subject;

        public override CheckResult Evaluate(PlacedPiece piece, EmotionContext context)
        {
            bool passed = subject != null && subject.Matches(piece, context);
            return new CheckResult(passed);
        }

        public override string GetDescription()
        {
            if (subject == null) return "(no check configured)";
            // subject.GetDescription() returns a noun phrase like "Red pieces" / "pieces on a Zone"
            // When used as a when-clause we want "being <phrase>" or "matching <phrase>" — "being"
            // reads naturally for most of the filter phrases we have.
            return $"being {subject.GetDescription()}";
        }
    }
}
