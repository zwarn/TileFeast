using Modes;
using Roguelike;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Roguelike
{
    // The panel GameObject stays enabled so event subscriptions persist.
    // contentRoot is shown/hidden to control visibility of the draft UI.
    public class RoguelikeDraftPanel : MonoBehaviour
    {
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private Transform offerContainer;
        [SerializeField] private RoguelikeDraftOfferEntry offerEntryPrefab;
        [SerializeField] private Button backButton;

        [Inject] private RoguelikeModeController _controller;
        [Inject] private DiContainer _diContainer;

        private RoguelikeDraftGroup _currentGroup;

        private void Start()
        {
            backButton.onClick.AddListener(OnBack);
            contentRoot.SetActive(false);
        }

        private void OnEnable()
        {
            _controller.OnDraftGroupAvailable += ShowDraftGroup;
        }

        private void OnDisable()
        {
            _controller.OnDraftGroupAvailable -= ShowDraftGroup;
        }

        private void ShowDraftGroup(RoguelikeDraftGroup group)
        {
            _currentGroup = group;

            foreach (Transform child in offerContainer)
                Destroy(child.gameObject);

            for (var i = 0; i < group.Options.Count; i++)
            {
                var capturedIdx = i;
                var entryGo = _diContainer.InstantiatePrefab(offerEntryPrefab, offerContainer);
                entryGo.SetActive(true);
                var entry = entryGo.GetComponent<RoguelikeDraftOfferEntry>();
                entry.SetData(group.Options[i], () => OnPick(capturedIdx));
            }

            contentRoot.SetActive(true);
        }

        private void OnPick(int offerIdx)
        {
            contentRoot.SetActive(false);
            _controller.PickOffer(_currentGroup, offerIdx);
        }

        private void OnBack()
        {
            contentRoot.SetActive(false);
            _controller.PostponeDraft(_currentGroup);
        }
    }
}