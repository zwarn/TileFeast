using Core;
using UnityEngine;
using Zenject;

namespace Tools
{
    public class LockTool : DrawTool
    {
        [Inject] private GameController _gameController;

        protected override void Paint(Vector2Int position)
        {
            _gameController.LockTile(position, true);
        }

        protected override void Erase(Vector2Int position)
        {
            _gameController.LockTile(position, false);
        }
    }
}