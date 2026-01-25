using UnityEngine;

namespace Hand.Tool
{
    /// <summary>
    /// Base class for tools that paint/draw on the grid.
    /// Left mouse paints, right mouse erases. Both support dragging.
    /// </summary>
    public abstract class DrawTool : ToolBase
    {
        [SerializeField] protected Grid grid;

        private bool _isSelected;
        private bool _isPainting;
        private bool _isErasing;


        private void Update()
        {
            if (!_isSelected) return;

            HandleInput();
        }

        protected abstract void Paint(Vector2Int position);
        protected abstract void Erase(Vector2Int position);

        private void HandleInput()
        {
            // Left mouse - paint
            if (Input.GetMouseButtonDown(0))
            {
                _isPainting = true;
                Paint(GetGridPosition());
            }

            if (Input.GetMouseButton(0) && _isPainting)
            {
                Paint(GetGridPosition());
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isPainting = false;
            }

            // Right mouse - erase
            if (Input.GetMouseButtonDown(1))
            {
                _isErasing = true;
                Erase(GetGridPosition());
            }

            if (Input.GetMouseButton(1) && _isErasing)
            {
                Erase(GetGridPosition());
            }

            if (Input.GetMouseButtonUp(1))
            {
                _isErasing = false;
            }
        }

        public override void OnSelect()
        {
            _isSelected = true;
            gameObject.SetActive(true);
        }

        public override void OnDeselect()
        {
            _isSelected = false;
            _isPainting = false;
            _isErasing = false;
            gameObject.SetActive(false);
        }

        protected Vector2Int GetGridPosition()
        {
            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return (Vector2Int)grid.WorldToCell(worldPos);
        }
    }
}