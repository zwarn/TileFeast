using System.Collections.Generic;
using Board;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Rules.Placement
{
    public class PlacementRuleViewEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private Checkmark checkmark;

        [Inject] private HighlightController _highlightController;

        private PlacementRuleSO _rule;

        private void Update()
        {
            var text = _rule.GetText();

            descriptionLabel.text = text;
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
        }
    }
}