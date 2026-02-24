using System.Collections.Generic;
using Solver;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Solver
{
    public class SolverPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text triedLabel;
        [SerializeField] private TMP_Text foundLabel;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private Button solveButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Transform resultsContainer;
        [SerializeField] private Transform viewParent;
        [SerializeField] private SolverResultEntry resultEntryPrefab;

        [Inject] private SolverRunner _solverRunner;
        [Inject] private ToolController _toolController;

        private void OnEnable()
        {
            _toolController.OnToolChanged += UpdateVisibility;
            _solverRunner.OnSolverStarted += HandleSolverStarted;
            _solverRunner.OnSolverComplete += HandleSolverComplete;
            RefreshButtons();
        }

        private void OnDisable()
        {
            _toolController.OnToolChanged -= UpdateVisibility;
            _solverRunner.OnSolverStarted -= HandleSolverStarted;
            _solverRunner.OnSolverComplete -= HandleSolverComplete;
        }

        private void UpdateVisibility(ToolType toolType)
        {
            viewParent.gameObject.SetActive(toolType == ToolType.CalculateTool);

            if (_solverRunner.IsRunning && toolType != ToolType.CalculateTool)
            {
                _solverRunner.StopSolving();
            }
        }

        private void Update()
        {
            if (!_solverRunner.IsRunning) return;

            triedLabel.text = $"Tried: {_solverRunner.TriedCount:N0}";
            foundLabel.text = $"Found: {_solverRunner.FoundCount}";
        }

        public void OnSolveClicked()
        {
            _solverRunner.StartSolving();
        }

        public void OnStopClicked()
        {
            _solverRunner.StopSolving();
        }

        private void HandleSolverStarted()
        {
            statusLabel.text = "Solving...";
            triedLabel.text = "Tried: 0";
            foundLabel.text = "Found: 0";
            ClearResults();
            RefreshButtons();
        }

        private void HandleSolverComplete(IReadOnlyList<SolverResult> results)
        {
            statusLabel.text = $"Done. {results.Count} solution(s) found.";
            triedLabel.text = $"Tried: {_solverRunner.TriedCount:N0}";
            foundLabel.text = $"Found: {_solverRunner.FoundCount}";
            PopulateResults(results);
            RefreshButtons();
        }

        private void PopulateResults(IReadOnlyList<SolverResult> results)
        {
            ClearResults();
            for (int i = 0; i < results.Count; i++)
            {
                var entry = Instantiate(resultEntryPrefab, resultsContainer);
                entry.SetData(i + 1, results[i], _solverRunner.ApplySolution);
            }
        }

        private void ClearResults()
        {
            foreach (Transform child in resultsContainer)
                Destroy(child.gameObject);
        }

        private void RefreshButtons()
        {
            bool running = _solverRunner.IsRunning;
            solveButton.interactable = !running;
            stopButton.interactable = running;
        }
    }
}