using System.Collections.Generic;
using System.Linq;
using Piece.model;
using Scenario;
using Score;

namespace State
{
    public class GameState
    {
        public List<PlacedPiece> PlacedPieces;
        public List<PieceSO> AvailablePieces;
        public PieceSO PieceInHand;
        public List<ScoreRule> ScoreRules;

        public GameState(List<PlacedPiece> placedPieces, List<PieceSO> availablePieces, PieceSO pieceInHand,
            List<ScoreRule> scoreRules)
        {
            PlacedPieces = placedPieces;
            AvailablePieces = availablePieces;
            PieceInHand = pieceInHand;
            ScoreRules = scoreRules;
        }

        public GameState(ScenarioSO scenarioSO) :
            this(new(),
                scenarioSO.availablePieces.ToList(),
                null,
                scenarioSO.scoreRules.ToList())
        {
        }
    }
}