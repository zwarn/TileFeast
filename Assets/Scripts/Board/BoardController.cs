using System;
using System.Collections.Generic;
using System.Linq;
using Shape.model;
using UnityEngine;

namespace Board
{
    public class BoardController : MonoBehaviour
    {
        [SerializeField] public int width;
        [SerializeField] public int height;

        private readonly List<PlacedShape> _shapes = new();
        private readonly Dictionary<Vector2Int, PlacedShape> _shapesByPosition = new();

        public event Action<PlacedShape> OnShapePlaced;
        public event Action<PlacedShape> OnShapeRemoved;

        public List<PlacedShape> Shapes => _shapes.ToList();

        public bool PlaceShape(ShapeWithRotation newShape, Vector2Int position)
        {
            var shape = new PlacedShape(newShape.Shape, newShape.Rotation, position);
            if (!IsValid(shape.GetTilePosition()))
            {
                return false;
            }

            _shapes.Add(shape);
            shape.GetTilePosition().ForEach(pos => _shapesByPosition[pos] = shape);
            PlaceShapeEvent(shape);
            return true;
        }

        public bool RemoveShape(PlacedShape shape)
        {
            bool removed = _shapes.Remove(shape);
            if (!removed)
            {
                return false;
            }

            shape.GetTilePosition().ForEach(pos => _shapesByPosition.Remove(pos));
            RemoveShapeEvent(shape);
            return true;
        }

        public PlacedShape GetShape(Vector2Int position)
        {
            if (_shapesByPosition.TryGetValue(position, out var shape))
            {
                return shape;
            }

            return null;
        }

        public Dictionary<Vector2Int, PlacedShape> GetShapeByPosition()
        {
            return _shapesByPosition.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private bool IsValid(List<Vector2Int> tiles)
        {
            return tiles.TrueForAll(IsValid);
        }

        private bool IsValid(Vector2Int position)
        {
            return InBounds(position) && IsEmpty(position);
        }

        private bool InBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x < width &&
                   position.y >= 0 && position.y < height;
        }

        private bool IsEmpty(Vector2Int position)
        {
            _shapesByPosition.TryGetValue(position, out var shape);
            return shape == null;
        }

        private void PlaceShapeEvent(PlacedShape shape)
        {
            OnShapePlaced?.Invoke(shape);
        }

        private void RemoveShapeEvent(PlacedShape shape)
        {
            OnShapeRemoved?.Invoke(shape);
        }
    }
}