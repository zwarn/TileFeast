using Rules.EmotionRules;
using TMPro;
using UnityEngine;

namespace UI.Rules
{
    public class EmotionRuleViewEntry : MonoBehaviour
    {
        [SerializeField] private TMP_Text descriptionLabel;

        public void SetRule(EmotionRuleConfig config)
        {
            if (descriptionLabel != null) descriptionLabel.text = config.rule.GetDescription();
        }
    }
}
