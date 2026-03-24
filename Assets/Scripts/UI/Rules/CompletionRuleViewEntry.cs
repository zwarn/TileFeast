using Core;
using Rules;
using Rules.CompletionRules;
using TMPro;
using UI.Common;
using UnityEngine;
using Zenject;

namespace UI.Rules
{
    public class CompletionRuleViewEntry : MonoBehaviour
    {
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private TMP_Text progressLabel;
        [SerializeField] private Checkmark checkmark;

        [Inject] private GameController _gameController;

        private CompletionRuleConfig _config;
        private RulesController _controller;

        public void SetRule(CompletionRuleConfig config, RulesController controller)
        {
            _config = config;
            _controller = controller;
            if (descriptionLabel != null) descriptionLabel.text = config.rule.GetDescription();
            Refresh();
        }

        public void Refresh()
        {
            if (_config == null || _controller == null || _gameController.CurrentState == null) return;

            var result = _controller.LastResult;
            var state = _gameController.CurrentState;

            if (progressLabel != null) progressLabel.text = _config.rule.GetProgress(result, state);

            bool met = _config.rule.IsMet(result, state);
            checkmark.SetState(met);
        }
    }
}
