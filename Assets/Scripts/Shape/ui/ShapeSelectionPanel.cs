using System;
using System.Collections.Generic;
using Shape.controller;
using Shape.model;
using UnityEngine;
using Zenject;

namespace Shape.ui
{
    public class ShapeSelectionPanel : MonoBehaviour
    {
        [Inject] private DiContainer _container;
        [Inject] private ShapeSupplyController _shapeSupply;


        [SerializeField] private ShapeSelectionEntry prefab;
        [SerializeField] private Transform entryParent;

        private readonly Dictionary<ShapeSO, ShapeSelectionEntry> _entries = new();

        private void Start()
        {
            _shapeSupply.GetShapes().ForEach(AddEntry);
        }
        
        private void OnEnable()
        {
            _shapeSupply.OnShapeAdded += ShapeAdded;
            _shapeSupply.OnShapeRemoved += ShapeRemoved;
        }

        private void OnDisable()
        {
            _shapeSupply.OnShapeAdded -= ShapeAdded;
            _shapeSupply.OnShapeRemoved -= ShapeRemoved;
        }

        private void ShapeRemoved(ShapeSO shape)
        {
            var entry = _entries[shape];
            Destroy(entry.gameObject);
            _entries.Remove(shape);
        }

        private void ShapeAdded(ShapeSO shape)
        {
            AddEntry(shape);
        }
        
        
        private void AddEntry(ShapeSO shape)
        {
            var entryObject = _container.InstantiatePrefab(prefab, entryParent);
            var entry = entryObject.GetComponent<ShapeSelectionEntry>();
            entry.SetData(shape);
            _entries.Add(shape, entry);
        }
    }
}