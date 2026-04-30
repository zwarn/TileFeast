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

        public void SetData(RoguelikeDraftOffer offer, Action onPick)
        {
            if (offerImage != null) offerImage.sprite = offer.PreviewSprite;
            if (nameText != null) nameText.text = offer.DisplayName;
            _onPick = onPick;
        }

        private void OnClick() => _onPick?.Invoke();
    }
}
