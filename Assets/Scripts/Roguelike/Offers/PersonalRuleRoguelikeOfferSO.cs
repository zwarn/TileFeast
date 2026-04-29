using Placeables.PersonalRulePlacements;
using Rules.EmotionRules;
using UnityEngine;

namespace Roguelike.Offers
{
    [CreateAssetMenu(fileName = "PersonalRuleOffer", menuName = "Roguelike/Offers/Personal Rule")]
    public class PersonalRuleRoguelikeOfferSO : RoguelikeOfferSO
    {
        [SerializeReference] public EmotionRule rule = new();

        public override bool IsMandatoryPlacement => true;

        public override void Apply(RoguelikeApplyContext context)
        {
            var placeable = new PersonalRulePlacement(rule, context.PersonalRuleSettings.icon, context.GameController);
            context.SupplyController.AddItem(placeable);
        }
    }
}
