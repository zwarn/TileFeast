using System.Collections.Generic;
using Solver;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Toggle = UnityEngine.UI.Toggle;

namespace UI.Solver
{
    public class SolverPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text triedLabel;
        [SerializeField] private TMP_Text foundLabel;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private Button solveButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Toggle forwardCheckToggle;
        [SerializeField] private Toggle fullCoverageToggle;
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

            if (forwardCheckToggle != null)
            {
                _solverRunner.ForwardCheckEnabled = forwardCheckToggle.isOn;
                forwardCheckToggle.onValueChanged.AddListener(OnForwardCheckChanged);
            }
            if (fullCoverageToggle != null)
            {
                _solverRunner.FullCoverageEnabled = fullCoverageToggle.isOn;
                fullCoverageToggle.onValueChanged.AddListener(OnFullCoverageChanged);
            }

            RefreshButtons();
        }

        private void OnDisable()
        {
            _toolController.OnToolChanged -= UpdateVisibility;
            _solverRunner.OnSolverStarted -= HandleSolverStarted;
            _solverRunner.OnSolverComplete -= HandleSolverComplete;

            if (forwardCheckToggle != null) forwardCheckToggle.onValueChanged.RemoveListener(OnForwardCheckChanged);
            if (fullCoverageToggle != null) fullCoverageToggle.onValueChanged.RemoveListener(OnFullCoverageChanged);
        }

        private void OnForwardCheckChanged(bool value) => _solverRunner.ForwardCheckEnabled = value;
        private void OnFullCoverageChanged(bool value) => _solverRunner.FullCoverageEnabled = value;

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

            triedLabel.text = _solverRunner.FoundCount > 0
                ? $"Best: {_solverRunner.BestScore}"
                : "Best: —";
            foundLabel.text = $"Found: {_solverRunner.FoundCount}";

            long tried = _solverRunner.TopLevelTriedCount;
            long total = _solverRunner.TotalTopLevelPositions;
            string progress = total > 0
                ? $"{tried:N0}/{total:N0} ({100.0 * tried / total:F1}%)"
                : "—";
            statusLabel.text = $"Solving... {progress}";
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
            triedLabel.text = "Best: —";
            foundLabel.text = "Found: 0";
            ClearResults();
            RefreshButtons();
        }

        private void HandleSolverComplete(IReadOnlyList<SolverResult> results)
        {
            statusLabel.text = $"Done. {results.Count} solution(s) found.";
            triedLabel.text = results.Count > 0 ? $"Best: {results[0].Score}" : "Best: —";
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