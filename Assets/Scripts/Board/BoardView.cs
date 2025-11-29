using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Zenject;

namespace Board
{
    public class BoardView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
    {
        [SerializeField] private Tilemap boardTilemap;
        [SerializeField] private Tilemap zoneTilemap;
        [SerializeField] private TileBase boardTile;
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

            UpdateBoardTilemap(gameState, width, height);
            UpdateZoneTilemap(gameState);
        }

        private void UpdateZoneTilemap(GameState gameState)
        {
            zoneTilemap.ClearAllTiles();
            
            gameState.Zones.ForEach(zone =>
            {
                zone.positions.ForEach(pos =>
                {
                    zoneTilemap.SetTile((Vector3Int)pos, zone.zoneType.zoneTile);
                });
            });
            
            
        }

        private void UpdateBoardTilemap(GameState gameState, int width, int height)
        {
            boardTilemap.ClearAllTiles();

            List<TileChangeData> tileChanges = new List<TileChangeData>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int current = new Vector2Int(x, y);
                    tileChanges.Add(new TileChangeData((Vector3Int)current, IncludeTile(gameState, current), Color.white,
                        Matrix4x4.identity));
                }
            }

            boardTilemap.SetTiles(tileChanges.ToArray(), true);
        }

        private TileBase IncludeTile(GameState gameState, Vector2Int current)
        {
            return !gameState.BlockedPositions.Contains(current) ? boardTile : null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != 0) return;

            var worldClickPoint = Camera.main.ScreenToWorldPoint(eventData.position);
            var position = (Vector2Int) grid.WorldToCell(worldClickPoint);

            _gameController.BoardClicked(position);
        }

        public void OnDrop(PointerEventData eventData)
        {
            var worldClickPoint = Camera.main.ScreenToWorldPoint(eventData.position);
            var position = (Vector2Int) grid.WorldToCell(worldClickPoint);

            _gameController.BoardClicked(position);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var worldClickPoint = Camera.main.ScreenToWorldPoint(eventData.position);
            var position = (Vector2Int) grid.WorldToCell(worldClickPoint);

            _gameController.BoardClicked(position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
        }
    }
}