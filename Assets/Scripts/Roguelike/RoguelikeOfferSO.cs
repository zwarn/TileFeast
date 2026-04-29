using UnityEngine;

namespace Roguelike
{
    public abstract class RoguelikeOfferSO : ScriptableObject
    {
        public Sprite previewSprite;
        public string displayName;

        // Non-piece placeables (BoardExpansion, WallPlacement, ZonePlacement, PersonalRule)
        // must be placed before the turn can end.
        public abstract bool IsMandatoryPlacement { get; }

        public abstract void Apply(RoguelikeApplyContext context);
    }
}
