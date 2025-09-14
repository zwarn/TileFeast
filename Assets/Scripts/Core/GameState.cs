using System.Collections.Generic;
using System.Linq;
using Piece;
using Rules.Placement;
using Rules.Score;
using Scenario;

namespace Core
{
    public class GameState
    {
        public List<PieceSO> AvailablePieces;
        public PieceSO PieceInHand;
        public List<PlacedPiece> PlacedPieces;
        public List<ScoreRuleSO> ScoreRules;
        public List<PlacementRuleSO> PlacementRules;

        public GameState(List<PlacedPiece> placedPieces, List<PieceSO> availablePieces, PieceSO pieceInHand,
            List<ScoreRuleSO> scoreRules, List<PlacementRuleSO> placementRules)
        {
            PlacedPieces = placedPieces;
            AvailablePieces = availablePieces;
            PieceInHand = pieceInHand;
            ScoreRules = scoreRules;
            PlacementRules = placementRules;
        }

        public GameState(ScenarioSO scenarioSO) :
            this(new List<PlacedPiece>(),
                scenarioSO.availablePieces.ToList(),
                null,
                scenarioSO.scoreRules.ToList(),
                scenarioSO.placementRules.ToList())
        {
        }
    }
}