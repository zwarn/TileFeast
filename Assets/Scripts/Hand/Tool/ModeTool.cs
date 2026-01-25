using UnityEngine;

namespace Hand.Tool
{
    /// <summary>
    /// Base class for tools that activate UI panels/modes.
    /// Shows the mode panel on select, hides on deselect.
    /// Override Update() if the mode needs custom input handling.
    /// </summary>
    public abstract class ModeTool : ToolBase
    {
        [SerializeField] protected GameObject modePanel;

        public override void OnSelect()
        {
            if (modePanel != null)
                modePanel.SetActive(true);
        }

        public override void OnDeselect()
        {
            if (modePanel != null)
                modePanel.SetActive(false);
        }
    }
}