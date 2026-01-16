using UnityEngine;

namespace Hand.Tool
{
    public class ToolController : MonoBehaviour
    {
        [SerializeField] public GrabTool grabTool;

        private ITool currentTool;

        private void Start()
        {
            ChangeTool(grabTool);
        }
        
        private void Update()
        {
            var targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition.z = 0;
            transform.localPosition = targetPosition;
        }

        public void ChangeTool(ITool newTool)
        {
            currentTool?.OnDeselect();
            currentTool = newTool;
            currentTool?.OnSelect();
        }

        public void LeftClicked(Vector2Int position)
        {
            if (currentTool == null) return;

            currentTool.OnLeftClick(position);
        }

        public void RightClicked(Vector2Int position)
        {
            if (currentTool == null) return;

            currentTool.OnRightClick(position);
        }

        public void Rotate(int direction)
        {
            if (currentTool == null) return;

            currentTool.OnRotate(direction);
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
            return ReferenceEquals(currentTool, grabTool);
        }
    }
}