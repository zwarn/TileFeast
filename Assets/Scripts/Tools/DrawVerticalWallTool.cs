using Core;
using UnityEngine;
using Zenject;

namespace Tools
{
    public class DrawVerticalWallTool : DrawTool
    {
        [Inject] private GameController _gameController;

        // Snap to the nearest vertical tile boundary.
        // Use WorldToCell for grid-aware tile lookup, then check which half of the tile
        // the cursor is in to determine whether the wall is to the right or left.
        protected override Vector2Int GetGridPosition()
        {
            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var cell = (Vector2Int)grid.WorldToCell(worldPos);
            var cellCenter = grid.GetCellCenterWorld((Vector3Int)cell);
            // Cursor in right half → wall on right edge (wallX = cx); left half → wall on left edge (wallX = cx-1)
            var wallX = worldPos.x >= cellCenter.x ? cell.x : cell.x - 1;
            return new Vector2Int(wallX, cell.y);
        }

        protected override void Paint(Vector2Int position)
        {
            _gameController.AddVerticalWall(position);
        }

        protected override void Erase(Vector2Int position)
        {
            _gameController.RemoveVerticalWall(position);
        }
    }
}
