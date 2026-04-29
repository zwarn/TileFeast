using Core;
using Pieces.Supply;
using Placeables.BoardExpansions;
using Placeables.PersonalRulePlacements;

namespace Roguelike
{
    public class RoguelikeApplyContext
    {
        public GameController GameController;
        public PieceSupplyController SupplyController;
        public BoardExpansionPreviewSettings BoardExpansionSettings;
        public PersonalRulePlacementSettings PersonalRuleSettings;
    }
}
