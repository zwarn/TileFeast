using UnityEngine;

namespace Placeables.PersonalRulePlacements
{
    public class PersonalRulePlacementView : MonoBehaviour
    {
        private static readonly Color ValidColor   = new(1f, 1f,    1f,    0.6f);
        private static readonly Color InvalidColor = new(1f, 0.35f, 0.35f, 0.6f);

        [SerializeField] private SpriteRenderer spriteRenderer;

        public void SetData(PersonalRulePlacement placement)
        {
            spriteRenderer.sprite = placement.PreviewSprite;
        }

        public void SetPreviewValid(bool valid)
        {
            spriteRenderer.color = valid ? ValidColor : InvalidColor;
        }
    }
}
