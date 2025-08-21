using Board;
using Shape.controller;
using Shape.model;
using UnityEngine;
using Zenject;

namespace Hand
{
    public class InteractionController : MonoBehaviour
    {
        [Inject] private BoardController _boardController;
        [Inject] private HandController _handController;
        [Inject] private ShapeSupplyController _shapeSupply;

        public void BoardClicked(Vector2Int position)
        {
            if (_handController.IsEmpty())
            {
                Grab(position);
            }
            else
            {
                Put(position);
            }
        }

        private void Put(Vector2Int position)
        {
            var shape = _handController.GetShape();
            bool success = _boardController.PlaceShape(shape, position);
            if (success)
            {
                _handController.FreeShape();
            }
        }

        private void Grab(Vector2Int position)
        {
            if (!_handController.IsEmpty()) return;
            var placedShape = _boardController.GetShape(position);
            if (placedShape == null) return;
            _boardController.RemoveShape(placedShape);
            _handController.SetShape(new ShapeWithRotation(placedShape.Shape, placedShape.Rotation));
        }

        public void ShapeSelectionClicked(ShapeSO shape)
        {
            if (!_handController.IsEmpty()) return;
            _handController.SetShape(new ShapeWithRotation(shape, 0));
            _shapeSupply.RemoveShape(shape);
        }
    }
}