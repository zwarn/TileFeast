using UnityEngine;

namespace Hand.Tool
{
    public interface ITool
    {

        void OnSelect();
        void OnDeselect();
        
        void OnLeftClick(Vector2Int position);
        void OnRightClick(Vector2Int position);

        void OnRotate(int direction);

        // OnLeftDown
        // OnRightDown


    }
}