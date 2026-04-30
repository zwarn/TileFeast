using System.Collections.Generic;
using System.Linq;
using Rules.EmotionRules;
using UnityEngine;

namespace Roguelike.Generators
{
    [CreateAssetMenu(fileName = "RuleOfferGenerator", menuName = "Roguelike/Generators/Rule Generator")]
    public class RuleOfferGeneratorSO : ScriptableObject
    {
        [SerializeReference] public List<EmotionRule> ruleTemplates;

        public List<RoguelikeDraftOffer> GenerateGroup(int count)
        {
            if (ruleTemplates == null || ruleTemplates.Count == 0)
            {
                Debug.LogWarning("[RuleOfferGeneratorSO] ruleTemplates is empty.");
                return new List<RoguelikeDraftOffer>();
            }

            return ruleTemplates.OrderBy(_ => Random.value)
                .Take(count)
                .Select(rule => new RoguelikeDraftOffer
                {
                    DisplayName = rule.GetDescription(),
                    Type = DraftOfferType.Rule,
                    Rule = rule,
                })
                .ToList();
        }
    }
}