using Core;
using Tools;
using UnityEngine;

namespace Pieces
{
    public class PlaceablePiece : IPlaceable
    {
        public readonly PieceWithRotation Piece;
        private readonly PieceView _view;
        private readonly GameController _gameController;

        public PlaceablePiece(PieceWithRotation piece, PieceView view, GameController gameController)
        {
            Piece = piece;
            _view = view;
            _gameController = gameController;
            view.SetData(piece);
        }

        public Sprite PreviewSprite => Piece.Piece.previewSprite != null ? Piece.Piece.previewSprite : Piece.Piece.sprite;
        public GameObject PreviewObject => _view.gameObject;

        // No-op: PieceView follows the ToolController transform automatically.
        public void UpdatePreview(Vector2Int boardCell) { }

        public bool TryPlace(Vector2Int boardCell)
            => _gameController.PlacePieceInHand(Piece, boardCell);

        public void Rotate(int direction)
            => Piece.Rotate(direction);

        public void OnDiscard()
        {
            _view.SetData(null);
            _gameController.ReturnPieceToSupply(Piece.Piece);
        }
    }
}
