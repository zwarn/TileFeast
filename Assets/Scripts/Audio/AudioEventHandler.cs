using System.Collections.Generic;
using Board;
using Core;
using Pieces;
using Pieces.Supply;
using Rules;
using Scenarios;
using Solver;
using Tools;
using UnityEngine;
using Zenject;

namespace Audio
{
    public class AudioEventHandler : MonoBehaviour
    {
        [Inject] private SoundController _sound;
        [Inject] private MusicController _music;
        [Inject] private BoardController _board;
        [Inject] private GameController _game;
        [Inject] private RulesController _rules;
        [Inject] private ToolController _tools;
        [Inject] private SolverRunner _solver;
        [Inject] private PieceSupplyController _supply;
        [Inject] private ScenarioController _scenario;

        private bool _wasLevelComplete;
        private EmotionEvaluationResult _lastResult;
        private bool _justPlaced;

        private void OnEnable()
        {
            _board.OnPiecePlaced += HandlePiecePlaced;
            _board.OnBoardReset += HandleBoardReset;
            _game.OnChangeGameState += HandleGameStateChanged;
            _game.OnHandChanged += HandleHandChanged;
            _game.OnPieceRotated += HandlePieceRotated;
            _rules.OnEvaluationChanged += HandleEvaluation;
            _tools.OnToolChanged += HandleToolChanged;
            _solver.OnSolverStarted += HandleSolverStarted;
            _solver.OnSolverComplete += HandleSolverComplete;
            _supply.OnPieceAdded += HandlePieceAdded;
        }

        private void OnDisable()
        {
            _board.OnPiecePlaced -= HandlePiecePlaced;
            _board.OnBoardReset -= HandleBoardReset;
            _game.OnChangeGameState -= HandleGameStateChanged;
            _game.OnHandChanged -= HandleHandChanged;
            _game.OnPieceRotated -= HandlePieceRotated;
            _rules.OnEvaluationChanged -= HandleEvaluation;
            _tools.OnToolChanged -= HandleToolChanged;
            _solver.OnSolverStarted -= HandleSolverStarted;
            _solver.OnSolverComplete -= HandleSolverComplete;
            _supply.OnPieceAdded -= HandlePieceAdded;
        }

        private void HandlePiecePlaced(PlacedPiece _)
        {
            _justPlaced = true;
            _sound.Play(GameSoundEvent.PiecePlaced);
        }

        private void HandleBoardReset(List<PlacedPiece> _)
        {
            _sound.Play(GameSoundEvent.BoardReset);
        }

        private void HandleHandChanged()
        {
            if (_game.GetPieceInHand() != null)
            {
                _sound.Play(GameSoundEvent.PiecePickedUp);
            }
            else if (!_justPlaced)
            {
                _sound.Play(GameSoundEvent.PieceReturned);
            }
            _justPlaced = false;
        }

        private void HandleGameStateChanged(GameState _)
        {
            _wasLevelComplete = false;
            _lastResult = null;
            _justPlaced = false;
            _sound.Play(GameSoundEvent.LevelLoaded);

            var ctx = _scenario.CurrentScenario != null
                ? _scenario.CurrentScenario.musicContext
                : MusicContext.MainMenu;
            _music.SetContext(ctx);
        }

        private void HandleEvaluation(EmotionEvaluationResult result)
        {
            int prevHappy = _lastResult?.HappyCount ?? 0;
            int prevSad = _lastResult?.SadCount ?? 0;
            int prevNeutral = _lastResult?.NeutralCount ?? 0;

            if (result.HappyCount > prevHappy) _sound.Play(GameSoundEvent.PieceBecameHappy);
            if (result.SadCount > prevSad) _sound.Play(GameSoundEvent.PieceBecameSad);
            if (result.NeutralCount > prevNeutral) _sound.Play(GameSoundEvent.PieceBecameNeutral);

            bool complete = _rules.IsLevelComplete();
            if (complete && !_wasLevelComplete)
            {
                _sound.Play(GameSoundEvent.LevelComplete);
                _music.SetContext(MusicContext.Victory);
            }
            _wasLevelComplete = complete;
            _lastResult = result;
        }

        private void HandlePieceRotated() => _sound.Play(GameSoundEvent.PieceRotated);

        private void HandleToolChanged(ToolType _) => _sound.Play(GameSoundEvent.ToolChanged);

        private void HandleSolverStarted() => _sound.Play(GameSoundEvent.SolverStarted);

        private void HandleSolverComplete(IReadOnlyList<SolverResult> _) => _sound.Play(GameSoundEvent.SolverComplete);

        private void HandlePieceAdded(Piece _) => _sound.Play(GameSoundEvent.PieceAddedToSupply);
    }
}
