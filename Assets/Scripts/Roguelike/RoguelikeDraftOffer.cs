using Rules.EmotionRules;
using Tools;
using UnityEngine;

namespace Roguelike
{
    public enum DraftOfferType { Piece, Placeable, Rule }

    public class RoguelikeDraftOffer
    {
        public string DisplayName;
        public Sprite PreviewSprite;
        public DraftOfferType Type;
        public IPlaceable Placeable;  // set for Piece and Placeable types
        public EmotionRule Rule;       // set for Rule type
    }
}
