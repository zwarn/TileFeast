using UnityEngine;

namespace Hand.Tool
{
    /// <summary>
    /// Abstract base class for all tools. Provides serialization support for Unity
    /// and contains the shared ToolSO data field.
    /// </summary>
    public abstract class ToolBase : MonoBehaviour
    {
        [SerializeField] protected ToolSO data;

        public ToolSO Data => data;

        public abstract void OnSelect();
        public abstract void OnDeselect();
    }
}
