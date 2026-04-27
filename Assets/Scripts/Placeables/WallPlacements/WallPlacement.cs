using System.Collections.Generic;
using System.Linq;
using Core;
using Tools;
using UnityEngine;

namespace Placeables.WallPlacements
{
    public class WallPlacement : IPlaceable
    {
        private readonly WallPlacementData _data;
        private readonly GameController _gameController;
        private int _rotation;

        public WallPlacement(WallPlacementData data, WallPlacementPreviewGenerator generator,
            GameController gameController)
        {
            _data = data;
            _gameController = gameController;
            PreviewSprite = generator.Generate(data);
        }

        // Same H↔V swap and coordinate rotation as BoardExpansion, but normalisation
        // uses wall positions directly (no shape tiles to derive it from).

        public List<Vector2Int> CurrentHorizontalWalls
        {
            get
            {
                var off = WallNormOffset();
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
                var off = WallNormOffset();
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

        // Integer centre of the normalised wall bounding box (used by TryPlace and WallPlacementView).
        public Vector2Int CurrentCenter
        {
            get
            {
                var all = CurrentHorizontalWalls.Concat(CurrentVerticalWalls).ToList();
                var maxX = all.Count > 0 ? all.Max(p => p.x) : 0;
                var maxY = all.Count > 0 ? all.Max(p => p.y) : 0;
                return new Vector2Int(maxX / 2, maxY / 2);
            }
        }

        public Sprite PreviewSprite { get; }

        public bool TryPlace(Vector2Int boardCell)
        {
            if (!IsValidPlacement(boardCell)) return false;

            var offset = boardCell - CurrentCenter;
            var absHWalls = CurrentHorizontalWalls.Select(w => w + offset).ToList();
            var absVWalls = CurrentVerticalWalls.Select(w => w + offset).ToList();
            _gameController.PlaceWalls(absHWalls, absVWalls);
            return true;
        }

        public void Rotate(int direction)
        {
            _rotation = ((_rotation + direction) % 4 + 4) % 4;
        }

        public void OnDiscard()
        {
            _gameController.ReturnToSupply(this);
        }

        public bool IsValidPlacement(Vector2Int boardCell)
        {
            var offset = boardCell - CurrentCenter;
            var absHWalls = CurrentHorizontalWalls.Select(w => w + offset).ToList();
            var absVWalls = CurrentVerticalWalls.Select(w => w + offset).ToList();
            return _gameController.IsWallPlacementValid(absHWalls, absVWalls);
        }

        // Minimum (x,y) across all raw rotated wall positions, used as normalisation origin.
        // Both CurrentHorizontalWalls and CurrentVerticalWalls subtract this same offset so
        // all walls stay spatially coherent after normalisation.
        private Vector2Int WallNormOffset()
        {
            var raw = new List<Vector2Int>();
            switch (_rotation)
            {
                case 0:
                    raw.AddRange(_data.HorizontalWalls);
                    raw.AddRange(_data.VerticalWalls);
                    break;
                case 1:
                    raw.AddRange(_data.VerticalWalls.Select(w => new Vector2Int(-w.y, w.x)));
                    raw.AddRange(_data.HorizontalWalls.Select(w => new Vector2Int(-w.y - 1, w.x)));
                    break;
                case 2:
                    raw.AddRange(_data.HorizontalWalls.Select(w => new Vector2Int(-w.x, -w.y - 1)));
                    raw.AddRange(_data.VerticalWalls.Select(w => new Vector2Int(-w.x - 1, -w.y)));
                    break;
                case 3:
                    raw.AddRange(_data.VerticalWalls.Select(w => new Vector2Int(w.y, -w.x - 1)));
                    raw.AddRange(_data.HorizontalWalls.Select(w => new Vector2Int(w.y, -w.x)));
                    break;
            }

            if (raw.Count == 0) return Vector2Int.zero;
            return new Vector2Int(raw.Min(p => p.x), raw.Min(p => p.y));
        }
    }
}