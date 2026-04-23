using UnityEngine;

namespace Tools
{
    public interface IPlaceable
    {
        Sprite PreviewSprite { get; }
        GameObject PreviewObject { get; }

        // Called every frame while held; allows the placeable to update its preview position.
        void UpdatePreview(Vector2Int boardCell);

        // Executes placement. Returns true on success.
        bool TryPlace(Vector2Int boardCell);

        void Rotate(int direction);

        // Called when the player discards the item (right-click / tool deselect).
        // Responsible only for cleanup (return to source, clear view data).
        // Hand state is cleared by the caller before OnDiscard is invoked.
        void OnDiscard();
    }
}
