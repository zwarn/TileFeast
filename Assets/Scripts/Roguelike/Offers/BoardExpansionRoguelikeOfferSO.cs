using Placeables.BoardExpansions;
using UnityEngine;

namespace Roguelike.Offers
{
    [CreateAssetMenu(fileName = "BoardExpansionOffer", menuName = "Roguelike/Offers/Board Expansion")]
    public class BoardExpansionRoguelikeOfferSO : RoguelikeOfferSO
    {
        public BoardExpansionData data;

        public override bool IsMandatoryPlacement => true;

        public override void Apply(RoguelikeApplyContext context)
        {
            var generator = new BoardExpansionPreviewGenerator(context.BoardExpansionSettings);
            var placeable = new BoardExpansion(data, generator, context.GameController);
            context.SupplyController.AddItem(placeable);
        }
    }
}
