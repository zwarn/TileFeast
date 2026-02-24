using System;
using Solver;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Solver
{
    public class SolverResultEntry : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TMP_Text rankLabel;
        [SerializeField] private TMP_Text scoreLabel;
        [SerializeField] private TMP_Text statusLabel;

        private SolverResult _result;
        private Action<SolverResult> _onApply;

        public void SetData(int rank, SolverResult result, Action<SolverResult> onApply)
        {
            _result = result;
            _onApply = onApply;
            rankLabel.text = $"#{rank}";
            scoreLabel.text = result.Score.ToString();
            statusLabel.text = result.RulesSatisfied ? "All rules satisfied" : "Rules violated";
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onApply?.Invoke(_result);
        }
    }
}
