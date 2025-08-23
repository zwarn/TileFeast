using System;
using TMPro;
using UnityEngine;

namespace Score
{
    public class ScoreViewEntry : MonoBehaviour
    {
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private TMP_Text scoreLabel;

        private ScoreCondition _condition;

        public void SetData(ScoreCondition condition)
        {
            _condition = condition;
        }

        private void Update()
        {
            var score = _condition.GetScore();
            var text = _condition.GetText();

            descriptionLabel.text = text;
            scoreLabel.text = score.ToString();
        }
    }
}