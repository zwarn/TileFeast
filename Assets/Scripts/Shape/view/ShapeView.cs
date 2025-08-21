using System;
using Shape.model;
using UnityEngine;

namespace Shape.view
{
    public class ShapeView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private ShapeWithRotation _shape;

        public void SetData(ShapeWithRotation shape)
        {
            _shape = shape;
            gameObject.SetActive(_shape != null);

            if (_shape != null)
            {
                spriteRenderer.sprite = shape.Shape.sprite;
                transform.rotation = Quaternion.Euler(0, 0, 90 * _shape.Rotation);
            }
        }

        private void Update()
        {
            transform.rotation = Quaternion.Euler(0, 0, 90 * _shape.Rotation);
        }
    }
}