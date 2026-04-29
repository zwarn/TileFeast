using Modes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Roguelike
{
    public class RoguelikeHUDPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Button viewPendingDraftsButton;
        [SerializeField] private TMP_Text pendingDraftsText;

        [Inject] private RoguelikeModeController _controller;

        private void OnEnable()
        {
            _controller.OnHealthChanged += UpdateHealth;
            _controller.OnEndTurnAllowedChanged += UpdateEndTurnButton;
            _controller.OnPendingDraftCountChanged += UpdatePendingDrafts;
            _controller.OnGameOver += HandleGameOver;
            endTurnButton.onClick.AddListener(OnEndTurn);
            viewPendingDraftsButton.onClick.AddListener(OnViewPendingDrafts);
        }

        private void OnDisable()
        {
            _controller.OnHealthChanged -= UpdateHealth;
            _controller.OnEndTurnAllowedChanged -= UpdateEndTurnButton;
            _controller.OnPendingDraftCountChanged -= UpdatePendingDrafts;
            _controller.OnGameOver -= HandleGameOver;
            endTurnButton.onClick.RemoveListener(OnEndTurn);
            viewPendingDraftsButton.onClick.RemoveListener(OnViewPendingDrafts);
        }

        private void UpdateHealth(int health)
        {
            if (healthText != null)
                healthText.text = $"Health: {health}";
        }

        private void UpdateEndTurnButton(bool allowed)
        {
            if (endTurnButton != null)
                endTurnButton.interactable = allowed;
        }

        private void UpdatePendingDrafts(int count)
        {
            if (viewPendingDraftsButton != null)
                viewPendingDraftsButton.gameObject.SetActive(count > 0);
            if (pendingDraftsText != null)
                pendingDraftsText.text = $"Choices : {count}";
        }

        private void HandleGameOver()
        {
            if (endTurnButton != null)
                endTurnButton.interactable = false;
        }

        private void OnEndTurn() => _controller.EndTurn();

        private void OnViewPendingDrafts() => _controller.ShowNextPendingDraft();
    }
}
