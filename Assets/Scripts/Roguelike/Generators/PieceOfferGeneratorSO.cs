using System.Collections.Generic;
using System.Linq;
using Core;
using Pieces;
using UnityEngine;

namespace Roguelike.Generators
{
    [CreateAssetMenu(fileName = "PieceOfferGenerator", menuName = "Roguelike/Generators/Piece Generator")]
    public class PieceOfferGeneratorSO : ScriptableObject
    {
        public List<PieceSO> pool;

        public List<RoguelikeDraftOffer> GenerateGroup(int count, GameController gc)
        {
            if (pool == null || pool.Count == 0)
            {
                Debug.LogWarning("[PieceOfferGeneratorSO] pool is empty.");
                return new List<RoguelikeDraftOffer>();
            }

            return pool.OrderBy(_ => Random.value)
                .Take(count)
                .Select(so => CreateOffer(so, gc))
                .ToList();
        }

        private static RoguelikeDraftOffer CreateOffer(PieceSO so, GameController gc)
        {
            var piece = new Piece(so, false);
            var placeable = new PlaceablePiece(new PieceWithRotation(piece, 0), gc);
            return new RoguelikeDraftOffer
            {
                DisplayName = so.name,
                PreviewSprite = so.previewSprite != null ? so.previewSprite : so.sprite,
                Type = DraftOfferType.Piece,
                Placeable = placeable,
            };
        }
    }
}