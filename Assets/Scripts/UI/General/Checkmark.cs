using UnityEngine;

namespace UI.General
{
    public class Checkmark : MonoBehaviour
    {
        [SerializeField] private bool initialState;
        [SerializeField] private Transform enabledView;
        [SerializeField] private Transform disabledView;

        private bool _isEnabled;

        private void Start()
        {
            _isEnabled = initialState;
            UpdateView();
        }

        public void SetState(bool isEnabled)
        {
            _isEnabled = isEnabled;
            UpdateView();
        }

        private void UpdateView()
        {
            enabledView.gameObject.SetActive(_isEnabled);
            disabledView.gameObject.SetActive(!_isEnabled);
        }
    }
}