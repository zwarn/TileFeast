using Shape.model;
using UnityEngine;

namespace Shape.ui
{
    public class AspectView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer renderer;

        public void SetData(AspectSO aspect, SortingLayer targetSorting)
        {
            renderer.sprite = aspect.image;
            renderer.sortingLayerID = targetSorting.id;
        }
    }
}