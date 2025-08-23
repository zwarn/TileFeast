using Shape.model;
using UnityEngine;

namespace Score
{
    
    public abstract class ScoreCondition : ScriptableObject
    {
        public abstract int GetScore();
        public abstract void CalculateScore(ShapeSO[,] tiles);
    }
}