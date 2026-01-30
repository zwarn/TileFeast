using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Board.Zone
{
    public class ZoneView : MonoBehaviour
    {
        [SerializeField] private Tilemap zoneTilemap;

        [Inject] private ZoneController _zoneController;

        private void OnEnable()
        {
            _zoneController.OnZonesReset += RedrawAllZones;
            _zoneController.OnZoneTilesChanged += UpdateTiles;
        }

        private void OnDisable()
        {
            _zoneController.OnZonesReset -= RedrawAllZones;
            _zoneController.OnZoneTilesChanged -= UpdateTiles;
        }

        private void RedrawAllZones()
        {
            zoneTilemap.ClearAllTiles();

            foreach (var zone in _zoneController.Zones)
            {
                foreach (var pos in zone.positions)
                {
                    zoneTilemap.SetTile((Vector3Int)pos, zone.zoneType.zoneTile);
                }
            }
        }

        private void UpdateTiles(List<Vector2Int> positions)
        {
            foreach (var pos in positions)
            {
                var zone = _zoneController.GetZone(pos);
                var tile = zone?.zoneType.zoneTile;
                zoneTilemap.SetTile((Vector3Int)pos, tile);
            }
        }
    }
}