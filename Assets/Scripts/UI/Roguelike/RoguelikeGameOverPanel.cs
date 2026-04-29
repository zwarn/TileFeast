using Modes;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI.Roguelike
{
    // The panel GameObject stays enabled so the event subscription persists.
    // contentRoot is shown when the game ends.
    public class RoguelikeGameOverPanel : MonoBehaviour
    {
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private TMP_Text messageText;

        [Inject] private RoguelikeModeController _controller;

        private void Start()
        {
            contentRoot.SetActive(false);
        }

        private void OnEnable()
        {
            _controller.OnGameOver += Show;
        }

        private void OnDisable()
        {
            _controller.OnGameOver -= Show;
        }

        private void Show()
        {
            contentRoot.SetActive(true);
            if (messageText != null)
                messageText.text = "Game Over!";
        }
    }
}
