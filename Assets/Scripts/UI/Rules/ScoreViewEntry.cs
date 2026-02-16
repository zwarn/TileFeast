using Board;
using Rules;
using Rules.ScoreRules;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace UI.Rules
{
    public class ScoreViewEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private TMP_Text scoreLabel;

        [Inject] private HighlightController _highlightController;

        private ScoreRuleSO _rule;

        private void Update()
        {
            var score = _rule.GetScore();
            var text = _rule.GetText();

            descriptionLabel.text = text;
            scoreLabel.text = score.ToString();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _highlightController.SetHighlight(_rule.GetScoreArea());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _highlightController.SetHighlight(HighlightData.Empty());
        }

        public void SetData(ScoreRuleSO rule)
        {
            _rule = rule;
        }
    }
}