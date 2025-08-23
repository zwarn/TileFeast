using Hand;
using Shape.model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Shape.ui
{
    public class ShapeSelectionEntry : MonoBehaviour, IPointerClickHandler
    {
        [Inject] private InteractionController _interactionController;

        [SerializeField] private Image image;

        private ShapeSO _shape;

        public void SetData(ShapeSO shape)
        {
            _shape = shape;

            image.sprite = shape.sprite;
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            _interactionController.GrabShapeFromSupply(_shape);
        }
    }
}