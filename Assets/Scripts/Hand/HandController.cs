using System;
using Board;
using Shape.model;
using Shape.view;
using UnityEngine;
using Zenject;

namespace Hand
{
    public class HandController : MonoBehaviour
    {
        [Inject] private BoardController _boardController;

        [SerializeField] private ShapeView shapeView;

        private ShapeWithRotation _currentShape;

        private void Start()
        {
            FreeShape();
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Q))
            {
                _currentShape.Rotate(1);
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                _currentShape.Rotate(-1);
            }

            var targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition.z = 0;
            transform.localPosition = targetPosition;
        }

        public bool IsEmpty()
        {
            return _currentShape == null;
        }

        public ShapeWithRotation GetShape()
        {
            return _currentShape;
        }

        public void FreeShape()
        {
            _currentShape = null;
            shapeView.SetData(_currentShape);
        }

        public void SetShape(ShapeWithRotation shape)
        {
            if (!IsEmpty())
            {
                Debug.LogError("Tried to override held shape");
                return;
            }
            _currentShape = shape;
            shapeView.SetData(_currentShape);
        }
    }
}