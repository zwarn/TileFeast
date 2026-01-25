using UnityEngine;

namespace Hand.Tool
{
    [CreateAssetMenu(fileName = "Tool", menuName = "Tool", order = 0)]
    public class ToolSO : ScriptableObject
    {
        public Sprite icon;
        public ToolType type;
    }
}