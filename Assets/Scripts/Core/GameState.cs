using System.Collections.Generic;
using System.Linq;
using Piece;
using Scenario;
using Score;

namespace Core
{
    public class GameState
    {
        public List<PieceSO> AvailablePieces;
        public PieceSO PieceInHand;
        public List<PlacedPiece> PlacedPieces;
        public List<ScoreRuleSO> ScoreRules;

        public GameState(List<PlacedPiece> placedPieces, List<PieceSO> availablePieces, PieceSO pieceInHand,
            List<ScoreRuleSO> scoreRules)
        {
            PlacedPieces = placedPieces;
            AvailablePieces = availablePieces;
            PieceInHand = pieceInHand;
            ScoreRules = scoreRules;
        }

        public GameState(ScenarioSO scenarioSO) :
            this(new List<PlacedPiece>(),
                scenarioSO.availablePieces.ToList(),
                null,
                scenarioSO.scoreRules.ToList())
        {
        }
    }
}