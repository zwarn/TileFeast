using System;
using Hand.Tool;
using UnityEngine;
using Zenject;

namespace Board
{
    public class BoardResizeButtons : MonoBehaviour
    {
        [Inject] private ToolController _toolController;

        [SerializeField] private Transform panel;

        private void OnEnable()
        {
            _toolController.OnToolChanged += ToolChanged;
        }

        private void OnDisable()
        {
            _toolController.OnToolChanged -= ToolChanged;
        }

        private void ToolChanged(ToolType toolType)
        {
            panel.gameObject.SetActive(toolType == ToolType.ResizeTool);
        }
    }
}