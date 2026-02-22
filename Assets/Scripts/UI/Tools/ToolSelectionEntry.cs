using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace UI.Tools
{
    public class ToolSelectionEntry : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private ToolButton toolButton;

        [Inject] private ToolController _toolController;

        private ToolType _toolType;

        private void OnEnable()
        {
            _toolController.OnToolChanged += ToolSelectionChanged;
            ToolSelectionChanged(_toolController.CurrentToolType);
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