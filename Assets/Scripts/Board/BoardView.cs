using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Zenject;

namespace Board
{
    public class BoardView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private TileBase tile;
        [SerializeField] private Grid grid;
        [SerializeField] private BoxCollider2D gridCollider;

        [Inject] private GameController _gameController;

        private void OnEnable()
        {
            _gameController.OnChangeGameState += UpdateState;
        }

        private void OnDisable()
        {
            _gameController.OnChangeGameState -= UpdateState;
        }

        private void UpdateState(GameState gameState)
        {
            var width = gameState.GridSize.x;
            var height = gameState.GridSize.y;

            grid.transform.position = new Vector3(width / 2f, height / 2f, 0);

            gridCollider.size = new Vector2(width, height);
            gridCollider.offset = new Vector2(width, height);

            tilemap.ClearAllTiles();

            List<TileChangeData> tileChanges = new List<TileChangeData>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int current = new Vector2Int(x, y);
                    tileChanges.Add(new TileChangeData((Vector3Int) current, IncludeTile(gameState, current), Color.white, Matrix4x4.identity));
                }
            }

            tilemap.SetTiles(tileChanges.ToArray(), true);
        }

        private TileBase IncludeTile(GameState gameState, Vector2Int current)
        {
            return !gameState.BlockedPositions.Contains(current) ? tile : null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != 0) return;

            var worldClickPoint = Camera.main.ScreenToWorldPoint(eventData.position);
            var position = (Vector2Int) grid.WorldToCell(worldClickPoint);

            _gameController.BoardClicked(position);
        }
    }
}