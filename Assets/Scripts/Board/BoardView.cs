using System;
using System.Collections.Generic;
using Hand;
using Shape;
using Shape.model;
using Shape.view;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Board
{
    public class BoardView : MonoBehaviour, IPointerClickHandler
    {
        [Inject] private DiContainer _container;
        [Inject] private BoardController _boardController;
        [Inject] private InteractionController _interactionController;

        [SerializeField] private ShapeView shapeViewPrefab;
        [SerializeField] private Transform shapeViewParent;

        private Dictionary<PlacedShape, ShapeView> _views = new();

        private void OnEnable()
        {
            _boardController.OnShapePlaced += ShapePlaced;
            _boardController.OnShapeRemoved += ShapeRemoved;
        }

        private void OnDisable()
        {
            _boardController.OnShapePlaced -= ShapePlaced;
            _boardController.OnShapeRemoved -= ShapeRemoved;
        }

        private void ShapePlaced(PlacedShape shape)
        {
            var viewObject = _container.InstantiatePrefab(shapeViewPrefab);
            viewObject.transform.parent = shapeViewParent;
            var shapeView = viewObject.GetComponent<ShapeView>();
            shapeView.SetData(new ShapeWithRotation(shape.Shape, shape.Rotation));
            shapeView.transform.position = new Vector3(shape.Position.x, shape.Position.y);
            _views.Add(shape, shapeView);
        }

        private void ShapeRemoved(PlacedShape shape)
        {
            var shapeView = _views[shape];
            _views.Remove(shape);
            Destroy(shapeView.gameObject);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var worldClickPoint = Camera.main.ScreenToWorldPoint(eventData.position);
            var localPosition = Vector2Int.RoundToInt(worldClickPoint - transform.position);

            _interactionController.BoardClicked(localPosition);
        }
    }
}