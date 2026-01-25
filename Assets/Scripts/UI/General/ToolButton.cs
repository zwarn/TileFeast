using UnityEngine;
using UnityEngine.UI;

namespace UI.General
{
    public class ToolButton : MonoBehaviour
    {
        [SerializeField] private Image buttonPressed;
        [SerializeField] private Image buttonUnpressed;
        [SerializeField] private Image icon;
        [SerializeField] private int iconPressedOffset = -2;

        private bool _pressed = false;

        private void Start()
        {
            UpdateState();
        }

        public void SetIcon(Sprite sprite)
        {
            icon.sprite = sprite;
        }

        public void SetPressed(bool pressed)
        {
            _pressed = pressed;
            UpdateState();
        }

        private void UpdateState()
        {
            buttonPressed.gameObject.SetActive(_pressed);
            buttonUnpressed.gameObject.SetActive(!_pressed);
            icon.gameObject.transform.localPosition = new Vector3(0, _pressed ? iconPressedOffset : 0, 0);
        }
    }
}