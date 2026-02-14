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

        public ZoneSO SelectedZoneType { get; private set; }
        private Zone _activeZone;

        public override void OnSelect()
        {
            base.OnSelect();
            SetZoneType(null);
        }

        public void SetZoneType(ZoneSO zoneType)
        {
            SelectedZoneType = zoneType;
        }

        protected override void Paint(Vector2Int position)
        {
            if (SelectedZoneType == null) return;

            if (_activeZone == null)
            {
                var existingZone = _zoneController.GetZone(position);
                if (existingZone != null && existingZone.zoneType == SelectedZoneType)
                    _activeZone = existingZone;
            }

            _activeZone = _gameController.PaintZoneTile(position, SelectedZoneType, _activeZone);
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