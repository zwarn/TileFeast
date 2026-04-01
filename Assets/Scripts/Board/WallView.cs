using Core;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Board
{
    public class WallView : MonoBehaviour
    {
        [SerializeField] private Tilemap horizontalWallTilemap;
        [SerializeField] private Tilemap verticalWallTilemap;
        [SerializeField] private TileBase horizontalWallTile;
        [SerializeField] private TileBase verticalWallTile;

        [Inject] private GameController _gameController;

        private GameState _gameState;

        private void OnEnable()
        {
            _gameController.OnChangeGameState += OnChangeGameState;
            _gameController.OnBoardChanged += Refresh;
        }

        private void OnDisable()
        {
            _gameController.OnChangeGameState -= OnChangeGameState;
            _gameController.OnBoardChanged -= Refresh;
        }

        private void OnChangeGameState(GameState gameState)
        {
            _gameState = gameState;
            Refresh();
        }

        private void Refresh()
        {
            if (_gameState == null) return;

            horizontalWallTilemap.ClearAllTiles();
            verticalWallTilemap.ClearAllTiles();

            // Horizontal wall (x, y): top edge of tile (x, y). Place tile at (x, y+1) in a
            // tilemap that is offset -0.5 in Y so the tile centers on the shared edge.
            foreach (var wall in _gameState.HorizontalWalls)
                horizontalWallTilemap.SetTile(new Vector3Int(wall.x, wall.y + 1, 0), horizontalWallTile);

            // Vertical wall (x, y): right edge of tile (x, y). Place tile at (x+1, y) in a
            // tilemap that is offset -0.5 in X so the tile centers on the shared edge.
            foreach (var wall in _gameState.VerticalWalls)
                verticalWallTilemap.SetTile(new Vector3Int(wall.x + 1, wall.y, 0), verticalWallTile);
        }
    }
}
