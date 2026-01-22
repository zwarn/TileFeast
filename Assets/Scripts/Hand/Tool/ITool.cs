using UnityEngine;

namespace Hand.Tool
{
    public interface ITool
    {
        void OnSelect();
        void OnDeselect();
        public Sprite Icon { get; }
    }
}
