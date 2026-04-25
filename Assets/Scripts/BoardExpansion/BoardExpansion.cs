using System.Collections.Generic;
using System.Linq;
using Core;
using Pieces;
using Tools;
using UnityEngine;
using Zones;

namespace BoardExpansion
{
    public class BoardExpansion : IPlaceable
    {
        private readonly BoardExpansionData _data;
        private readonly GameController _gameController;
        private readonly Sprite _previewSprite;
        private int _rotation;

        public BoardExpansion(BoardExpansionData data, BoardExpansionPreviewGenerator generator, GameController gameController)
        {
            _data = data;
            _gameController = gameController;
            _previewSprite = generator.Generate(data);
        }

        public Sprite PreviewSprite => _previewSprite;

        public List<Vector2Int> CurrentShape =>
            ShapeHelper.Normalize(ShapeHelper.Rotate(_data.Shape, _rotation));

        // Walls swap type (H↔V) at 90°/270° rotations and their positions transform accordingly.
        // Rotation convention: 1 = 90°CW (x,y)→(−y,x), 2 = 180° (−x,−y), 3 = 270°CW (y,−x).

        public List<Vector2Int> CurrentHorizontalWalls
        {
            get
            {
                var off = NormOffset();
                return _rotation switch
                {
                    0 => _data.HorizontalWalls.Select(w => w - off).ToList(),
                    1 => _data.VerticalWalls.Select(w => new Vector2Int(-w.y, w.x) - off).ToList(),
                    2 => _data.HorizontalWalls.Select(w => new Vector2Int(-w.x, -w.y - 1) - off).ToList(),
                    3 => _data.VerticalWalls.Select(w => new Vector2Int(w.y, -w.x - 1) - off).ToList(),
                    _ => new List<Vector2Int>()
                };
            }
        }

        public List<Vector2Int> CurrentVerticalWalls
        {
            get
            {
                var off = NormOffset();
                return _rotation switch
                {
                    0 => _data.VerticalWalls.Select(w => w - off).ToList(),
                    1 => _data.HorizontalWalls.Select(w => new Vector2Int(-w.y - 1, w.x) - off).ToList(),
                    2 => _data.VerticalWalls.Select(w => new Vector2Int(-w.x - 1, -w.y) - off).ToList(),
                    3 => _data.HorizontalWalls.Select(w => new Vector2Int(w.y, -w.x) - off).ToList(),
                    _ => new List<Vector2Int>()
                };
            }
        }

        public List<Zone> CurrentZones
        {
            get
            {
                if (_data.Zones.Count == 0) return _data.Zones;
                var off = NormOffset();
                return _data.Zones
                    .Select(z => new Zone(z.zoneType,
                        ShapeHelper.Rotate(z.positions, _rotation).Select(p => p - off).ToList()))
                    .ToList();
            }
        }

        // Integer center of the normalized bounding box (minX=0, minY=0).
        // Used by both TryPlace and BoardExpansionView so they stay in sync.
        public Vector2Int CurrentCenter
        {
            get
            {
                var shape = CurrentShape;
                int maxX = shape.Count > 0 ? shape.Max(p => p.x) : 0;
                int maxY = shape.Count > 0 ? shape.Max(p => p.y) : 0;
                return new Vector2Int(maxX / 2, maxY / 2);
            }
        }

        public bool IsValidPlacement(Vector2Int boardCell)
        {
            var offset = boardCell - CurrentCenter;
            var absoluteTiles = CurrentShape.Select(p => p + offset).ToList();
            var absoluteHWalls = CurrentHorizontalWalls.Select(w => w + offset).ToList();
            var absoluteVWalls = CurrentVerticalWalls.Select(w => w + offset).ToList();
            return _gameController.IsExpansionValid(absoluteTiles, absoluteHWalls, absoluteVWalls);
        }

        public bool TryPlace(Vector2Int boardCell)
        {
            if (!IsValidPlacement(boardCell)) return false;

            var offset = boardCell - CurrentCenter;
            var absoluteTiles  = CurrentShape.Select(p => p + offset).ToList();
            var absoluteHWalls = CurrentHorizontalWalls.Select(w => w + offset).ToList();
            var absoluteVWalls = CurrentVerticalWalls.Select(w => w + offset).ToList();
            var absoluteZones  = CurrentZones
                .Select(z => new Zone(z.zoneType, z.positions.Select(p => p + offset).ToList()))
                .ToList();
            _gameController.ExpandBoard(absoluteTiles, absoluteHWalls, absoluteVWalls, absoluteZones);
            return true;
        }

        public void Rotate(int direction)
        {
            _rotation = ((_rotation + direction) % 4 + 4) % 4;
        }

        public void OnDiscard()
            => _gameController.ReturnToSupply(this);

        private Vector2Int NormOffset()
        {
            var rotated = ShapeHelper.Rotate(_data.Shape, _rotation);
            if (rotated.Count == 0) return Vector2Int.zero;
            int minX = int.MaxValue, minY = int.MaxValue;
            foreach (var p in rotated)
            {
                if (p.x < minX) minX = p.x;
                if (p.y < minY) minY = p.y;
            }
            return new Vector2Int(minX, minY);
        }
    }
}
