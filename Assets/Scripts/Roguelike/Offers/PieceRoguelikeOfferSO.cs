using Pieces;
using UnityEngine;

namespace Roguelike.Offers
{
    [CreateAssetMenu(fileName = "PieceOffer", menuName = "Roguelike/Offers/Piece")]
    public class PieceRoguelikeOfferSO : RoguelikeOfferSO
    {
        public PieceSO piece;

        public override bool IsMandatoryPlacement => false;

        public override void Apply(RoguelikeApplyContext context)
        {
            var p = new Piece(piece, false);
            var placeable = new PlaceablePiece(new PieceWithRotation(p, 0), context.GameController);
            context.SupplyController.AddItem(placeable);
        }
    }
}
