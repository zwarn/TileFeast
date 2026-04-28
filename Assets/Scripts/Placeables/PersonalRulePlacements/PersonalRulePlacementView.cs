using Tools;
using UnityEngine;

namespace Placeables.PersonalRulePlacements
{
    public class PersonalRulePlacementView : MonoBehaviour, IPlaceableView
    {
        private static readonly Color ValidColor   = new(1f, 1f,    1f,    0.6f);
        private static readonly Color InvalidColor = new(1f, 0.35f, 0.35f, 0.6f);

        [SerializeField] private SpriteRenderer spriteRenderer;

        private PersonalRulePlacement _placeable;

        public void Bind(IPlaceable placeable)
        {
            _placeable = (PersonalRulePlacement)placeable;
            spriteRenderer.sprite = _placeable.PreviewSprite;
        }

        public void Activate()
        {
            transform.localPosition = Vector3.zero;
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            transform.localPosition = Vector3.zero;
            gameObject.SetActive(false);
        }

        public void UpdateFrame(Grid grid, Vector3 mouseWorldPos, Vector2Int boardCell)
        {
            var cellWorld = grid.CellToWorld(new Vector3Int(boardCell.x, boardCell.y, 0));
            transform.localPosition = new Vector3(
                cellWorld.x + 0.5f - mouseWorldPos.x,
                cellWorld.y + 0.5f - mouseWorldPos.y,
                transform.localPosition.z);
            spriteRenderer.color = _placeable.IsValidPlacement(boardCell) ? ValidColor : InvalidColor;
        }

        public void OnRotated() { }
    }
}
