using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Scenarios;
using UnityEngine;
using Zenject;

namespace Solver
{
    public class SolverRunner : MonoBehaviour
    {
        [Inject] private ScenarioController _scenarioController;
        [Inject] private GameController _gameController;

        [SerializeField] private int maxResults = 5;

        private AutoSolver _solver;
        private CancellationTokenSource _cts;
        private Task<List<SolverResult>> _solveTask;
        private List<SolverResult> _pendingResults;

        private List<SolverResult> _results = new();

        public long TriedCount => _solver != null ? Interlocked.Read(ref _solver.TriedCount) : 0;
        public int FoundCount => _solver?.FoundCount ?? 0;
        public bool IsRunning => _solveTask != null && !_solveTask.IsCompleted;
        public IReadOnlyList<SolverResult> Results => _results;
        public int MaxResults => maxResults;

        public event Action OnSolverStarted;
        public event Action<IReadOnlyList<SolverResult>> OnSolverComplete;

        public void StartSolving()
        {
            if (IsRunning) return;

            _results = new List<SolverResult>();

            // AutoSolver constructor clones ScriptableObjects — must run on main thread
            _solver = new AutoSolver(_scenarioController.CurrentScenario, maxResults);

            _cts = new CancellationTokenSource();
            _solveTask = _solver.SolveAsync(_cts.Token);
            _solveTask.ContinueWith(t =>
            {
                if (!t.IsFaulted && !t.IsCanceled)
                    _pendingResults = t.Result;
            });

            OnSolverStarted?.Invoke();
        }

        public void StopSolving()
        {
            _cts?.Cancel();
        }

        private void Update()
        {
            if (_pendingResults != null)
            {
                _results = _pendingResults;
                _pendingResults = null;
                OnSolverComplete?.Invoke(_results);
            }
        }

        public void ApplySolution(SolverResult result)
        {
            _gameController.ReturnAllNonLockedPiecesToSupply();

            foreach (var placement in result.Placements)
            {
                if (placement.IsLocked()) continue;

                var gamePiece = _gameController.CurrentState.AvailablePieces
                    .FirstOrDefault(p => p.sourceSO == placement.Piece.sourceSO);

                if (gamePiece == null) continue;

                _gameController.DeletePieceFromSupply(gamePiece);
                _gameController.SpawnPiece(gamePiece, placement.Position, placement.Rotation);
            }
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
        }
    }
}
