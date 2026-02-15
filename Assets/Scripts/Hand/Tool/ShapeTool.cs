using System.Collections.Generic;
using Board;
using Board.Zone;
using Core;
using Rules;
using UnityEngine;
using Zenject;

namespace Hand.Tool
{
    public class ShapeTool : DrawTool
    {
        [Inject] private GameController _gameController;
        [Inject] private HighlightController _highlightController;
        [Inject] private BoardController _boardController;

        private List<Vector2Int> _shapePositions;


        public override void OnSelect()
        {
            base.OnSelect();
            Reset();
        }

        public override void OnDeselect()
        {
            base.OnDeselect();
            Reset();
        }

        protected override void Paint(Vector2Int position)
        {
            if (!_shapePositions.Contains(position) && _boardController.IsValid(position))
            {
                _shapePositions.Add(position);
                UpdateShape();
            }
        }

        protected override void Erase(Vector2Int position)
        {
            if (_shapePositions.Contains(position))
            {
                _shapePositions.Remove(position);
                UpdateShape();
            }
        }

        private void Reset()
        {
            _shapePositions = new List<Vector2Int>();
            UpdateShape();
        }

        private void UpdateShape()
        {
            UpdateHighlight();
        }

        private void UpdateHighlight()
        {
            if (_shapePositions.Count == 0)
            {
                _highlightController.ResetHighlight();
            }
            else
            {
                _highlightController.SetHighlight(new HighlightData(Color.cyan, _shapePositions));
            }
        }
    }
}