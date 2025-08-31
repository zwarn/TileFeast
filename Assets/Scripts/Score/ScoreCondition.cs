using System;
using Piece.model;
using UnityEngine;

namespace Score
{
    
    public abstract class ScoreCondition : ScriptableObject
    {
        public abstract int GetScore();
        public abstract void CalculateScore(PieceSO[,] tiles);

        public abstract String GetText();
    }
}