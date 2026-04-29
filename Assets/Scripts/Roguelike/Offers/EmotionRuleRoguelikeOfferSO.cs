using Rules.EmotionRules;
using UnityEngine;

namespace Roguelike.Offers
{
    // Applying this offer immediately adds an EmotionRule to the board's global rules.
    // No placement is required; it does not count as a mandatory placeable.
    [CreateAssetMenu(fileName = "EmotionRuleOffer", menuName = "Roguelike/Offers/Emotion Rule")]
    public class EmotionRuleRoguelikeOfferSO : RoguelikeOfferSO
    {
        [SerializeReference] public EmotionRule rule = new();

        public override bool IsMandatoryPlacement => false;

        public override void Apply(RoguelikeApplyContext context)
        {
            context.GameController.AddEmotionRule(rule);
        }
    }
}
