using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Board
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private Tilemap boardTilemap;
        [SerializeField] private TileBase boardTile;
        [SerializeField] private Grid grid;
        [SerializeField] private BoxCollider2D gridCollider;

        [Inject] private GameController _gameController;

        private GameState _gameState;

        private void OnEnable()
        {
            _gameController.OnChangeGameState += UpdateState;
            _gameController.OnBoardChanged += RefreshBoardTilemap;
            _gameController.OnTileChanged += UpdateSingleTile;
        }

        private void OnDisable()
        {
            _gameController.OnChangeGameState -= UpdateState;
            _gameController.OnBoardChanged -= RefreshBoardTilemap;
            _gameController.OnTileChanged -= UpdateSingleTile;
        }

        private void UpdateState(GameState gameState)
        {
            _gameState = gameState;

            var width = gameState.GridSize.x;
            var height = gameState.GridSize.y;

            grid.transform.position = new Vector3(width / 2f, height / 2f, 0);

            gridCollider.size = new Vector2(width, height);
            gridCollider.offset = new Vector2(width, height);

            UpdateBoardTilemap(width, height);
        }

        private void RefreshBoardTilemap()
        {
            if (_gameState == null) return;
            UpdateBoardTilemap(_gameState.GridSize.x, _gameState.GridSize.y);
        }

        private void UpdateSingleTile(Vector2Int position)
        {
            if (_gameState == null) return;
            boardTilemap.SetTile((Vector3Int)position, GetTileAt(position));
        }

        private void UpdateBoardTilemap(int width, int height)
        {
            boardTilemap.ClearAllTiles();

            List<TileChangeData> tileChanges = new List<TileChangeData>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int current = new Vector2Int(x, y);
                    tileChanges.Add(new TileChangeData((Vector3Int)current, GetTileAt(current), Color.white,
                        Matrix4x4.identity));
                }
            }

            boardTilemap.SetTiles(tileChanges.ToArray(), true);
        }

        private TileBase GetTileAt(Vector2Int position)
        {
            return !_gameState.BlockedPositions.Contains(position) ? boardTile : null;
        }
    }
}
