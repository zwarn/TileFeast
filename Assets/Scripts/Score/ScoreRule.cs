using System;
using System.Collections.Generic;
using Piece.model;
using UnityEngine;

namespace Score
{
    
    public abstract class ScoreRule : ScriptableObject
    {
        public abstract int GetScore();
        public abstract void CalculateScore(PieceSO[,] tiles);

        public abstract List<Vector2Int> GetScoreArea();

        public abstract String GetText();
    }
}