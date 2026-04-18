using System;
using Pieces;
using Rules.Checks;
using Rules.Conclusions;
using Rules.Filters;

namespace Rules.EmotionRules
{
    /// <summary>
    /// Composable emotion rule: <see cref="filter"/> gates which pieces are considered,
    /// <see cref="check"/> tests a condition, and <see cref="conclusion"/> turns the
    /// result into an <see cref="EmotionEffect"/>.
    /// </summary>
    [Serializable]
    public class EmotionRule
    {
        [UnityEngine.SerializeReference]
        [UnityEngine.Tooltip("Which pieces this rule applies to")]
        public PieceFilter filter = new AllPiecesFilter();

        [UnityEngine.SerializeReference]
        [UnityEngine.Tooltip("Condition evaluated for each matching piece")]
        public EmotionCheck check = new AlwaysPassCheck();

        [UnityEngine.SerializeReference]
        [UnityEngine.Tooltip("How the check result is turned into an emotion")]
        public EmotionConclusion conclusion = new BinaryEmotionConclusion();

        public EmotionEffect Evaluate(PlacedPiece piece, EmotionContext context)
        {
            if (filter == null || !filter.Matches(piece, context)) return null;
            if (check == null || conclusion == null) return null;

            var result = check.Evaluate(piece, context);
            if (result == null) return null;

            return conclusion.Build(result, this, check);
        }

        public string GetDescription()
        {
            var filterText = filter != null ? filter.GetDescription() : "(no filter)";
            var conclusionText = conclusion != null ? conclusion.GetDescription() : "(no conclusion)";
            var checkText = check != null ? check.GetDescription() : "(no check)";
            return $"{filterText} are {conclusionText} when {checkText}";
        }
    }
}
