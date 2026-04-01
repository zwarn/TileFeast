using Core;
using UnityEngine;
using Zenject;

namespace Tools
{
    public class DrawHorizontalWallTool : DrawTool
    {
        [Inject] private GameController _gameController;

        protected override Vector2Int GetGridPosition()
        {
            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var cell = (Vector2Int)grid.WorldToCell(worldPos);
            var cellCenter = grid.GetCellCenterWorld((Vector3Int)cell);
            // Cursor in top half → wall on top edge (wallY = cy); bottom half → wall on bottom edge (wallY = cy-1)
            var wallY = worldPos.y >= cellCenter.y ? cell.y : cell.y - 1;
            return new Vector2Int(cell.x, wallY);
        }

        protected override void Paint(Vector2Int position)
        {
            _gameController.AddHorizontalWall(position);
        }

        protected override void Erase(Vector2Int position)
        {
            _gameController.RemoveHorizontalWall(position);
        }
    }
}
