using Modes;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.MainMenu
{
    public class MainMenuPanel : MonoBehaviour
    {
        [SerializeField] private Button puzzleModeButton;
        [SerializeField] private Button roguelikeModeButton;

        [Inject] private GameModeBootstrapper _bootstrapper;

        private void OnEnable()
        {
            puzzleModeButton.onClick.AddListener(OnPuzzleMode);
            roguelikeModeButton.onClick.AddListener(OnRoguelikeMode);
        }

        private void OnDisable()
        {
            puzzleModeButton.onClick.RemoveListener(OnPuzzleMode);
            roguelikeModeButton.onClick.RemoveListener(OnRoguelikeMode);
        }

        private void OnPuzzleMode()
        {
            _bootstrapper.StartPuzzleMode();
            gameObject.SetActive(false);
        }

        private void OnRoguelikeMode()
        {
            _bootstrapper.StartRoguelikeMode();
            gameObject.SetActive(false);
        }
    }
}
