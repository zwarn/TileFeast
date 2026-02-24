using System.Collections.Generic;
using Pieces;

namespace Solver
{
    public class SolverResult
    {
        public IReadOnlyList<PlacedPiece> Placements { get; }
        public int Score { get; }
        public bool RulesSatisfied { get; }

        public SolverResult(List<PlacedPiece> placements, int score, bool rulesSatisfied)
        {
            Placements = placements;
            Score = score;
            RulesSatisfied = rulesSatisfied;
        }
    }
}
