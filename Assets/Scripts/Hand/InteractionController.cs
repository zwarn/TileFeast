using System;
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

        public event Action OnBoardChanged;

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetKeyUp(KeyCode.Q))
            {
                _handController.Rotate(1);
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                _handController.Rotate(-1);
            }

            if (Input.GetMouseButtonUp(1))
            {
                ReturnShapeToSupply();
            }
        }

        public void BoardClicked(Vector2Int position)
        {
            if (_handController.IsEmpty())
            {
                GrabFromBoard(position);
            }
            else
            {
                PutOnBoard(position);
            }
        }

        private void PutOnBoard(Vector2Int position)
        {
            var shape = _handController.GetShape();
            bool success = _boardController.PlaceShape(shape, position);
            if (success)
            {
                _handController.FreeShape();
                BoardChangedEvent();
            }
        }

        private void GrabFromBoard(Vector2Int position)
        {
            if (!_handController.IsEmpty()) return;

            var placedShape = _boardController.GetShape(position);
            if (placedShape == null) return;

            _boardController.RemoveShape(placedShape);
            _handController.SetShape(new ShapeWithRotation(placedShape.Shape, placedShape.Rotation));
            BoardChangedEvent();
        }

        public void GrabShapeFromSupply(ShapeSO shape)
        {
            if (!_handController.IsEmpty()) return;

            _handController.SetShape(new ShapeWithRotation(shape, 0));
            _shapeSupply.RemoveShape(shape);
        }

        private void ReturnShapeToSupply()
        {
            if (_handController.IsEmpty()) return;

            var shape = _handController.GetShape();
            _shapeSupply.AddShape(shape);
            _handController.FreeShape();
        }

        public void BoardChangedEvent()
        {
            OnBoardChanged?.Invoke();
        }
    }
}