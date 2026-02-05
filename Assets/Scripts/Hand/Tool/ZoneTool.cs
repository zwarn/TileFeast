using Board.Zone;
using Core;
using UnityEngine;
using Zenject;

namespace Hand.Tool
{
    public class ZoneTool : DrawTool
    {
        [Inject] private GameController _gameController;
        [Inject] private ZoneController _zoneController;
        [SerializeField] private ZoneSO zoneType;

        private ZoneSO _selectedZoneType;
        private Zone _activeZone;

        private void Start()
        {
            SetZoneType(zoneType);
        }

        public void SetZoneType(ZoneSO zoneType)
        {
            _selectedZoneType = zoneType;
        }

        protected override void Paint(Vector2Int position)
        {
            if (_selectedZoneType == null) return;

            if (_activeZone == null)
            {
                var existingZone = _zoneController.GetZone(position);
                if (existingZone != null && existingZone.zoneType == _selectedZoneType)
                    _activeZone = existingZone;
            }

            _activeZone = _gameController.PaintZoneTile(position, _selectedZoneType, _activeZone);
        }

        protected override void OnPaintEnd()
        {
            _activeZone = null;
        }

        protected override void Erase(Vector2Int position)
        {
            _gameController.EraseZoneTile(position);
        }
    }
}