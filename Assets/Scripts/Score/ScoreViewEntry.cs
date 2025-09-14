using System.Collections.Generic;
using Board;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Score
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
            _highlightController.SetHighlight(_rule.GetScoreArea(), Color.cyan);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _highlightController.SetHighlight(new List<Vector2Int>(), Color.cyan);
        }

        public void SetData(ScoreRuleSO rule)
        {
            _rule = rule;
        }
    }
}