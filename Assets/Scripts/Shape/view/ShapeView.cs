using System;
using Shape.model;
using Shape.ui;
using UnityEngine;

namespace Shape.view
{
    public class ShapeView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private AspectListView aspectListView;

        private ShapeWithRotation _shape;

        public void SetData(ShapeWithRotation shape)
        {
            _shape = shape;
            gameObject.SetActive(_shape != null);

            if (_shape != null)
            {
                spriteRenderer.sprite = shape.Shape.sprite;
                transform.rotation = Quaternion.Euler(0, 0, 90 * _shape.Rotation);

                aspectListView.SetData(shape.Shape.aspects, shape.Shape);
            }
        }

        private void Update()
        {
            transform.rotation = Quaternion.Euler(0, 0, 90 * _shape.Rotation);
        }
    }
}