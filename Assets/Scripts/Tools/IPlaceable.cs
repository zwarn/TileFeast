using UnityEngine;

namespace Tools
{
    public interface IPlaceable
    {
        Sprite PreviewSprite { get; }
        bool TryPlace(Vector2Int boardCell);
        void Rotate(int direction);
        void OnDiscard();
    }
}
