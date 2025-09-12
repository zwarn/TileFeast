using System.Collections.Generic;
using System.Linq;
using State;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;
using Matrix4x4 = UnityEngine.Matrix4x4;

namespace Board
{
    public class HighlightController : MonoBehaviour
    {
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private TileBase highlightTile;

        [Inject] private GameController _gameController;

        private void OnEnable()
        {
            _gameController.OnBoardChanged += ResetHighlight;
        }

        private void OnDisable()
        {
            _gameController.OnBoardChanged -= ResetHighlight;
        }

        public void SetHighlight(List<Vector2Int> positions, Color color)
        {
            tilemap.ClearAllTiles();
            if (positions == null || positions.Count == 0) return;
            
            tilemap.SetTiles(
                positions.Select(pos =>
                        new TileChangeData(new Vector3Int(pos.x, pos.y, 0), highlightTile, color, Matrix4x4.identity))
                    .ToArray(), true);
        }

        private void ResetHighlight()
        {
            SetHighlight(new List<Vector2Int>(), Color.black);
        }
    }
}