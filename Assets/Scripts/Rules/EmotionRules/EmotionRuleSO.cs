using Pieces;
using UnityEngine;

namespace Rules.EmotionRules
{
    public abstract class EmotionRuleSO : ScriptableObject
    {
        // Returns the emotion effect this rule has on the given piece,
        // or null if the rule does not apply or produces no change (Neutral).
        public abstract EmotionEffect Evaluate(PlacedPiece piece, EmotionContext context, EmotionRuleArgs args);

        public abstract string GetDescription(EmotionRuleArgs args);
    }
}
