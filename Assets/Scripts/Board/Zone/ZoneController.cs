using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;
using Zenject;

namespace Board.Zone
{
    public class ZoneController : MonoBehaviour
    {
        [Inject] private GameController _gameController;

        public event Action OnZonesReset;
        public event Action<List<Vector2Int>> OnZoneTilesChanged;

        private readonly List<Zone> _zones = new();
        private readonly Dictionary<Vector2Int, Zone> _zonesByPosition = new();

        public List<Zone> Zones => _zones.ToList();

        private void OnEnable()
        {
            _gameController.OnChangeGameState += UpdateZones;
        }

        private void OnDisable()
        {
            _gameController.OnChangeGameState -= UpdateZones;
        }

        public Zone GetZone(Vector2Int position)
        {
            return _zonesByPosition.GetValueOrDefault(position);
        }

        public void AddZone(Zone zone)
        {
            if (zone.positions.Count == 0) return;

            // Deduplicate positions
            zone.positions = zone.positions.Distinct().ToList();

            // Remove these positions from any existing zones
            RemoveTilesFromZones(zone.positions);

            _zones.Add(zone);
            foreach (var pos in zone.positions)
            {
                _zonesByPosition[pos] = zone;
            }

            OnZoneTilesChanged?.Invoke(zone.positions);
        }

        public void RemoveZone(Zone zone)
        {
            if (!_zones.Remove(zone)) return;

            var positions = zone.positions.ToList();
            foreach (var pos in positions)
            {
                _zonesByPosition.Remove(pos);
            }

            zone.positions.Clear();

            OnZoneTilesChanged?.Invoke(positions);
        }

        public void AddTilesToZone(Zone zone, List<Vector2Int> positions)
        {
            if (positions.Count == 0) return;

            // Remove these positions from any other zones first
            RemoveTilesFromZones(positions);

            // If zone is not yet tracked, add it
            if (!_zones.Contains(zone))
            {
                _zones.Add(zone);
            }

            foreach (var pos in positions)
            {
                if (!zone.positions.Contains(pos))
                {
                    zone.positions.Add(pos);
                }

                _zonesByPosition[pos] = zone;
            }

            OnZoneTilesChanged?.Invoke(positions);
        }

        public void RemoveTilesFromZones(List<Vector2Int> positions)
        {
            foreach (var pos in positions)
            {
                if (!_zonesByPosition.TryGetValue(pos, out var zone)) continue;

                zone.positions.Remove(pos);
                _zonesByPosition.Remove(pos);

                if (zone.positions.Count == 0)
                {
                    _zones.Remove(zone);
                }
            }
            
            OnZoneTilesChanged?.Invoke(positions);
        }

        private void UpdateZones(GameState gameState)
        {
            _zones.Clear();
            _zonesByPosition.Clear();

            foreach (var zone in gameState.Zones)
            {
                zone.positions = zone.positions.Distinct().ToList();
                _zones.Add(zone);
                foreach (var pos in zone.positions)
                {
                    _zonesByPosition[pos] = zone;
                }
            }

            OnZonesReset?.Invoke();
        }

        public void HandleBoardResize(Vector2Int size, Vector2Int translate)
        {
            if (translate.magnitude > 0)
            {
                // Rebuild dictionary with translated positions
                _zonesByPosition.Clear();
                foreach (var zone in _zones)
                {
                    zone.positions = zone.positions.Select(oldPos => oldPos + translate).ToList();
                    foreach (var pos in zone.positions)
                    {
                        _zonesByPosition[pos] = zone;
                    }
                }
            }

            // Remove positions outside new bounds
            var removedPositions = _zonesByPosition.Keys
                .Where(pos => pos.x >= size.x || pos.y >= size.y || pos.x < 0 || pos.y < 0)
                .ToList();

            RemoveTilesFromZones(removedPositions);

            OnZonesReset?.Invoke();
        }
    }
}