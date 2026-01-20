using System;
using UnityEngine;

namespace Hand.Tool
{
    public class ToolController : MonoBehaviour
    {
        [SerializeField] public GrabTool grabTool;
        [SerializeField] public DrawAvailableTiles drawAvailableTiles;

        private ITool _currentTool;

        public event Action<ITool> OnToolChanged;

        public ITool CurrentTool => _currentTool;

        private void Start()
        {
            ChangeTool(grabTool);
        }

        private void Update()
        {
            var targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition.z = 0;
            transform.localPosition = targetPosition;

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ChangeTool(grabTool);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ChangeTool(drawAvailableTiles);
            }
        }

        public void ChangeTool(ITool newTool)
        {
            if (ReferenceEquals(_currentTool, newTool)) return;

            _currentTool?.OnDeselect();
            _currentTool = newTool;
            _currentTool?.OnSelect();

            OnToolChanged?.Invoke(_currentTool);
        }

        public GrabTool SelectGrabTool()
        {
            if (!IsHoldingGrabTool())
            {
                ChangeTool(grabTool);
            }

            return grabTool;
        }

        public bool IsHoldingGrabTool()
        {
            return ReferenceEquals(_currentTool, grabTool);
        }
    }
}
