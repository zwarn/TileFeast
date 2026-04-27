using System.Collections.Generic;
using System.Linq;
using Core;
using Pieces;
using Tools;
using UnityEngine;
using Zones;

namespace BoardExpansion
{
    public class ZonePlacement : IPlaceable
    {
        private readonly ZonePlacementData _data;
        private readonly GameController _gameController;
        private readonly Sprite _previewSprite;
        private int _rotation;

        public ZonePlacement(ZonePlacementData data, ZonePlacementPreviewGenerator generator, GameController gameController)
        {
            _data = data;
            _gameController = gameController;
            _previewSprite = generator.Generate(data);
        }

        public Sprite PreviewSprite => _previewSprite;

        public ZoneSO ZoneType => _data.ZoneType;

        public List<Vector2Int> CurrentShape =>
            ShapeHelper.Normalize(ShapeHelper.Rotate(_data.Shape, _rotation));

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
            return _gameController.IsZonePlacementValid(absoluteTiles);
        }

        public bool TryPlace(Vector2Int boardCell)
        {
            if (!IsValidPlacement(boardCell)) return false;

            var offset = boardCell - CurrentCenter;
            var absoluteTiles = CurrentShape.Select(p => p + offset).ToList();
            _gameController.ApplyZonePlacement(_data.ZoneType, absoluteTiles);
            return true;
        }

        public void Rotate(int direction)
        {
            _rotation = ((_rotation + direction) % 4 + 4) % 4;
        }

        public void OnDiscard()
            => _gameController.ReturnToSupply(this);
    }
}
