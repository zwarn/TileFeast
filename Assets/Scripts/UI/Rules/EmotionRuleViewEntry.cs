using Rules.EmotionRules;
using TMPro;
using UnityEngine;

namespace UI.Rules
{
    public class EmotionRuleViewEntry : MonoBehaviour
    {
        [SerializeField] private TMP_Text descriptionLabel;

        public void SetRule(EmotionRule rule)
        {
            if (descriptionLabel != null && rule != null)
                descriptionLabel.text = rule.GetDescription();
        }
    }
}
