using Roguelike;
using Scenarios;
using UnityEngine;
using Zenject;

namespace Modes
{
    public class GameModeBootstrapper : MonoBehaviour
    {
        [SerializeField] private ScenarioSO startPuzzleScenario;
        [SerializeField] private RoguelikeRunSO roguelikeRun;

        [Inject] private PuzzleModeController _puzzleModeController;
        [Inject] private RoguelikeModeController _roguelikeModeController;

        public void StartPuzzleMode()
        {
            _roguelikeModeController.enabled = false;
            _puzzleModeController.enabled = true;
            _puzzleModeController.StartPuzzle(startPuzzleScenario);
        }

        public void StartRoguelikeMode()
        {
            _puzzleModeController.enabled = false;
            _roguelikeModeController.enabled = true;
            _roguelikeModeController.StartRun(roguelikeRun);
        }
    }
}
