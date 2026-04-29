using Placeables.ZonePlacementS;
using UnityEngine;

namespace Roguelike.Offers
{
    [CreateAssetMenu(fileName = "ZonePlacementOffer", menuName = "Roguelike/Offers/Zone Placement")]
    public class ZonePlacementRoguelikeOfferSO : RoguelikeOfferSO
    {
        public ZonePlacementData data;

        public override bool IsMandatoryPlacement => true;

        public override void Apply(RoguelikeApplyContext context)
        {
            var generator = new ZonePlacementPreviewGenerator();
            var placeable = new ZonePlacement(data, generator, context.GameController);
            context.SupplyController.AddItem(placeable);
        }
    }
}
