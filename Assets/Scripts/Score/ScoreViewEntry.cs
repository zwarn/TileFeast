using System;
using TMPro;
using UnityEngine;

namespace Score
{
    public class ScoreViewEntry : MonoBehaviour
    {
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private TMP_Text scoreLabel;

        private ScoreRule _rule;

        public void SetData(ScoreRule rule)
        {
            _rule = rule;
        }

        private void Update()
        {
            var score = _rule.GetScore();
            var text = _rule.GetText();

            descriptionLabel.text = text;
            scoreLabel.text = score.ToString();
        }
    }
}