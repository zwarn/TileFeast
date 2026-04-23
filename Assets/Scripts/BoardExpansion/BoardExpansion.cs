using System.Collections.Generic;
using System.Linq;
using Core;
using Pieces;
using Tools;
using UnityEngine;

namespace BoardExpansion
{
    public class BoardExpansion : IPlaceable
    {
        private readonly BoardExpansionSO _data;
        private readonly BoardExpansionView _view;
        private readonly GameController _gameController;
        private int _rotation;

        public BoardExpansion(BoardExpansionSO data, BoardExpansionView view, GameController gameController)
        {
            _data = data;
            _view = view;
            _gameController = gameController;
            _view.SetShape(CurrentShape, _data.previewTile);
        }

        public Sprite PreviewSprite => _data.previewSprite;
        public GameObject PreviewObject => _view.gameObject;

        private List<Vector2Int> CurrentShape =>
            ShapeHelper.Normalize(ShapeHelper.Rotate(_data.shape, _rotation));

        public void UpdatePreview(Vector2Int boardCell) { }

        public bool TryPlace(Vector2Int boardCell)
        {
            var absoluteTiles = CurrentShape.Select(offset => offset + boardCell).ToList();
            _gameController.ExpandBoard(absoluteTiles);
            return true;
        }

        public void Rotate(int direction)
        {
            _rotation = ((_rotation + direction) % 4 + 4) % 4;
            _view.SetShape(CurrentShape, _data.previewTile);
        }

        public void OnDiscard()
        {
            // Preview is hidden by GrabTool.UpdatePlaceable via PreviewObject.SetActive(false).
        }
    }
}
