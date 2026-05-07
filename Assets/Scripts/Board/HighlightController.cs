using System.Collections.Generic;
using Rules;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Board
{
    public class HighlightController : MonoBehaviour
    {
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private TileBase highlightTile;

        public void SetHighlight(HighlightData highlightData)
        {
            tilemap.ClearAllTiles();
            if (highlightData.Groups == null || highlightData.Groups.Count == 0) return;

            var changes = new List<TileChangeData>();
            foreach (var group in highlightData.Groups)
            {
                if (group.Positions == null) continue;
                foreach (var pos in group.Positions)
                    changes.Add(new TileChangeData(new Vector3Int(pos.x, pos.y, 0), highlightTile, group.Color, Matrix4x4.identity));
            }

            if (changes.Count > 0)
                tilemap.SetTiles(changes.ToArray(), true);
        }

        public void ResetHighlight()
        {
            tilemap.ClearAllTiles();
        }
    }
}
