using System.Collections.Generic;
using Piece;
using UnityEngine;

namespace Score
{
    public abstract class ScoreRuleSO : ScriptableObject
    {
        public abstract int GetScore();
        public abstract void CalculateScore(ScoreContext scoreContext);

        public abstract List<Vector2Int> GetScoreArea();

        public abstract string GetText();
    }
}