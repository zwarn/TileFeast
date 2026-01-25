using UI.General;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Hand.Tool
{
    public class ToolSelectionEntry : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private ToolButton toolButton;

        [Inject] private ToolController _toolController;

        private ToolType _toolType;

        private void OnEnable()
        {
            _toolController.OnToolChanged += ToolSelectionChanged;
        }

        private void OnDisable()
        {
            _toolController.OnToolChanged -= ToolSelectionChanged;
        }

        private void ToolSelectionChanged(ToolType toolType)
        {
            toolButton.SetPressed(toolType == _toolType);
        }

        public void SetData(ToolSO toolSo)
        {
            _toolType = toolSo.type;
            toolButton.SetIcon(toolSo.icon);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _toolController.ChangeTool(_toolType);
        }
    }
}