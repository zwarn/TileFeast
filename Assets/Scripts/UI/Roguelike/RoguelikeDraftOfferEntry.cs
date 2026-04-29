using System;
using Roguelike;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Roguelike
{
    public class RoguelikeDraftOfferEntry : MonoBehaviour
    {
        [SerializeField] private Image offerImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Button button;

        private Action _onPick;

        private void OnEnable() => button.onClick.AddListener(OnClick);
        private void OnDisable() => button.onClick.RemoveListener(OnClick);

        public void SetData(RoguelikeOfferSO offer, Action onPick)
        {
            if (offerImage != null) offerImage.sprite = offer.previewSprite;
            if (nameText != null) nameText.text = offer.displayName;
            _onPick = onPick;
        }

        private void OnClick() => _onPick?.Invoke();
    }
}
