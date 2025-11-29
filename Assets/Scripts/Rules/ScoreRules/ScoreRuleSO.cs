using UnityEngine;

namespace Rules.ScoreRules
{
    public abstract class ScoreRuleSO : ScriptableObject
    {
        public abstract int GetScore();
        public abstract void CalculateScore(RuleContext context);

        public abstract HighlightData GetScoreArea();

        public abstract string GetText();
    }
}