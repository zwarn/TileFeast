using System;
using Shape.model;
using Shape.view;
using UnityEngine;

namespace Shape.controller
{
    public class ShapeController : MonoBehaviour
    {
        [SerializeField] private ShapeView shapeView;
        [SerializeField] private ShapeSO shape;

        private ShapeWithRotation _currentShape;

        private void Start()
        {
            _currentShape = new ShapeWithRotation(shape, 0);
            shapeView.SetData(_currentShape);
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
        }
    }
}