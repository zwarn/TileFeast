using Core;
using Tools;
using UnityEngine;

namespace Pieces
{
    public class PlaceablePiece : IPlaceable
    {
        public readonly PieceWithRotation Piece;
        private readonly GameController _gameController;

        public PlaceablePiece(PieceWithRotation piece, GameController gameController)
        {
            Piece = piece;
            _gameController = gameController;
        }

        public Sprite PreviewSprite => Piece.Piece.previewSprite != null ? Piece.Piece.previewSprite : Piece.Piece.sprite;

        public bool TryPlace(Vector2Int boardCell)
            => _gameController.PlacePieceInHand(Piece, boardCell);

        public void Rotate(int direction)
            => Piece.Rotate(direction);

        public void OnDiscard()
            => _gameController.ReturnToSupply(this);
    }
}
