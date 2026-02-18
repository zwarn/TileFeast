using Board;
using Core;
using UnityEngine;
using Zenject;

namespace Tools
{
    public class RemovePieceTool : DrawTool
    {
        [Inject] private GameController _gameController;
        [Inject] private BoardController _boardController;

        protected override void Paint(Vector2Int position)
        {
            // Do nothing on paint
        }

        protected override void Erase(Vector2Int position)
        {
            _gameController.DeletePieceFromBoard(position);
        }
    }
}
