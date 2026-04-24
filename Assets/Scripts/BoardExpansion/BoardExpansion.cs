using System.Collections.Generic;
using System.Linq;
using Core;
using Pieces;
using Tools;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BoardExpansion
{
    public class BoardExpansion : IPlaceable
    {
        private readonly BoardExpansionSO _data;
        private readonly GameController _gameController;
        private int _rotation;

        public BoardExpansion(BoardExpansionSO data, GameController gameController)
        {
            _data = data;
            _gameController = gameController;
        }

        public Sprite PreviewSprite => _data.previewSprite;
        public TileBase PreviewTile => _data.previewTile;
        public List<Vector2Int> CurrentShape =>
            ShapeHelper.Normalize(ShapeHelper.Rotate(_data.shape, _rotation));

        public bool TryPlace(Vector2Int boardCell)
        {
            var absoluteTiles = CurrentShape.Select(offset => offset + boardCell).ToList();
            _gameController.ExpandBoard(absoluteTiles);
            return true;
        }

        public void Rotate(int direction)
        {
            _rotation = ((_rotation + direction) % 4 + 4) % 4;
        }

        public void OnDiscard()
            => _gameController.ReturnToSupply(this);
    }
}
