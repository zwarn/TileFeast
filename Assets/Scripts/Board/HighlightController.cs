using System.Collections.Generic;
using System.Linq;
using Core;
using Rules;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

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

        public void SetHighlight(HighlightData highlightData)
        {
            tilemap.ClearAllTiles();
            if (highlightData.Positions == null || highlightData.Positions.Count == 0) return;

            tilemap.SetTiles(
                highlightData.Positions.Select(pos =>
                        new TileChangeData(new Vector3Int(pos.x, pos.y, 0), highlightTile, highlightData.Color, Matrix4x4.identity))
                    .ToArray(), true);
        }

        public void ResetHighlight()
        {
            SetHighlight(HighlightData.Empty());
        }
    }
}