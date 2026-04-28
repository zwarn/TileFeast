using UnityEngine;

namespace Tools
{
    public interface IPlaceableView
    {
        void Bind(IPlaceable placeable);
        void Activate();
        void Deactivate();
        void UpdateFrame(Grid grid, Vector3 mouseWorldPos, Vector2Int boardCell);
        void OnRotated();
    }
}
