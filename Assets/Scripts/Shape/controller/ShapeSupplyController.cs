using System;
using System.Collections.Generic;
using Shape.model;
using Shape.ui;
using UnityEngine;

namespace Shape.controller
{
    public class ShapeSupplyController : MonoBehaviour
    {
        [SerializeField] private List<ShapeSO> shapes;

        public event Action<ShapeSO> OnShapeAdded;
        public event Action<ShapeSO> OnShapeRemoved;

        public List<ShapeSO> GetShapes()
        {
            return shapes;
        }

        public void RemoveShape(ShapeSO shape)
        {
            shapes.Remove(shape);
            RemoveShapeEvent(shape);
        }

        public void AddShape(ShapeWithRotation shape)
        {
            shapes.Add(shape.Shape);
            AddShapeEvent(shape.Shape);
        }

        public void RemoveShapeEvent(ShapeSO shape)
        {
            OnShapeRemoved?.Invoke(shape);
        }

        public void AddShapeEvent(ShapeSO shape)
        {
            OnShapeAdded?.Invoke(shape);
        }
    }
}