using Placeables.WallPlacements;
using UnityEngine;

namespace Roguelike.Offers
{
    [CreateAssetMenu(fileName = "WallPlacementOffer", menuName = "Roguelike/Offers/Wall Placement")]
    public class WallPlacementRoguelikeOfferSO : RoguelikeOfferSO
    {
        public WallPlacementData data;

        public override bool IsMandatoryPlacement => true;

        public override void Apply(RoguelikeApplyContext context)
        {
            var generator = new WallPlacementPreviewGenerator(context.BoardExpansionSettings);
            var placeable = new WallPlacement(data, generator, context.GameController);
            context.SupplyController.AddItem(placeable);
        }
    }
}
