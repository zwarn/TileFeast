using Board;
using Rules;
using Rules.PlacementRules;
using TMPro;
using UI.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace UI.Rules
{
    public class PlacementRuleViewEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private Checkmark checkmark;

        [Inject] private HighlightController _highlightController;

        private PlacementRuleSO _rule;

        private void Update()
        {
            descriptionLabel.text = _rule.GetText();
            checkmark.SetState(_rule.IsSatisfied());
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _highlightController.SetHighlight(_rule.GetViolationSpots());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _highlightController.SetHighlight(HighlightData.Empty());
        }

        public void SetData(PlacementRuleSO rule)
        {
            _rule = rule;

            descriptionLabel.text = _rule.GetText();
            checkmark.SetState(_rule.IsSatisfied());
        }
    }
}