using Board;
using Rules;
using Rules.EmotionRules;
using TMPro;
using UI.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
using System;

namespace UI.Rules
{
    /// <summary>
    /// Displays a single EmotionRule as one TMP_Text with embedded link tags so individual
    /// keywords remain hoverable without splitting the label into multiple GameObjects.
    ///
    /// Link IDs embedded in the text:
    ///   "filter" → highlights filter-matching pieces + supply
    ///   "check"  → highlights check context (neighbors / groups)
    ///   (no link) → hovering anywhere else shows the rule's effect on placed pieces
    /// </summary>
    public class EmotionRuleViewEntry : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        private const string FilterLinkColor = "#7FD8E8"; // soft cyan  — marks filter keyword
        private const string CheckLinkColor  = "#FFD966"; // soft yellow — marks check keyword

        [SerializeField] private TMP_Text descriptionLabel;

        [Inject] private HighlightService _highlightService;
        [Inject] private TooltipService  _tooltipService;
        [Inject] private RulesController _rulesController;

        private EmotionRule _rule;
        private string      _hoveredLinkId;
        private string      _baseRichText;

        private const string HighlightMark = "<mark=#FFD96680>";

        private void OnEnable()
        {
            _highlightService.OnRuleUiHighlightChanged += OnRuleUiHighlightChanged;
            _highlightService.OnRuleUiHighlightCleared += OnRuleUiHighlightCleared;
        }

        private void OnDisable()
        {
            _highlightService.OnRuleUiHighlightChanged -= OnRuleUiHighlightChanged;
            _highlightService.OnRuleUiHighlightCleared -= OnRuleUiHighlightCleared;
        }

        private void OnRuleUiHighlightChanged(Predicate<EmotionRule> predicate)
        {
            if (descriptionLabel == null) return;
            descriptionLabel.text = predicate(_rule)
                ? $"{HighlightMark}{_baseRichText}</mark>"
                : _baseRichText;
        }

        private void OnRuleUiHighlightCleared()
        {
            if (descriptionLabel != null)
                descriptionLabel.text = _baseRichText;
        }

        public void SetRule(EmotionRule rule)
        {
            _rule = rule;
            if (descriptionLabel == null) return;
            _baseRichText = rule != null ? BuildRichText(rule) : string.Empty;
            descriptionLabel.text = _baseRichText;
        }

        // ── Pointer handlers ──────────────────────────────────────────────────────

        public void OnPointerEnter(PointerEventData eventData)
        {
            // OnPointerMove will immediately refine the board highlight; rule effects is the fallback.
            ShowRuleEffects();
            _tooltipService?.Show(new EmotionRuleTooltipData
            {
                Rule             = _rule,
                EvaluationResult = _rulesController.LastResult,
            });
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (_rule == null || descriptionLabel == null) return;

            // FindIntersectingLink returns -1 when no link is under the cursor.
            // Pass null as camera for Screen Space Overlay canvases.
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(
                descriptionLabel, Input.mousePosition, null);

            string linkId = linkIndex >= 0
                ? descriptionLabel.textInfo.linkInfo[linkIndex].GetLinkID()
                : null;

            if (linkId == _hoveredLinkId) return; // no change — avoid redundant calls
            _hoveredLinkId = linkId;

            switch (linkId)
            {
                case "filter": _highlightService?.HighlightFilter(_rule.filter);       break;
                case "check":  _highlightService?.HighlightCheckContext(_rule);         break;
                default:       ShowRuleEffects();                                       break;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hoveredLinkId = null;
            _highlightService?.ClearAll();
            _tooltipService?.Hide();
        }

        // ── Internal ──────────────────────────────────────────────────────────────

        private void ShowRuleEffects() => _highlightService?.HighlightRuleEffects(_rule);

        private static string BuildRichText(EmotionRule rule)
        {
            var filterText     = rule.filter?.GetDescription()     ?? "(no filter)";
            var conclusionText = rule.conclusion?.GetDescription() ?? "(no conclusion)";
            var checkText      = rule.check?.GetDescription()      ?? "(no check)";

            return $"<link=\"filter\"><color={FilterLinkColor}>{filterText}</color></link>" +
                   $" are {conclusionText} when" +
                   $" <link=\"check\"><color={CheckLinkColor}>{checkText}</color></link>";
        }
    }
}
