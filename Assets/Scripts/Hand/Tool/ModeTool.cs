using UnityEngine;

namespace Hand.Tool
{
    /// <summary>
    /// Base class for tools that activate UI panels/modes.
    /// Shows the mode panel on select, hides on deselect.
    /// Override Update() if the mode needs custom input handling.
    /// </summary>
    public abstract class ModeTool : MonoBehaviour, ITool
    {
        [SerializeField] protected GameObject modePanel;
        [SerializeField] protected Sprite icon;

        public Sprite Icon => icon;

        public virtual void OnSelect()
        {
            if (modePanel != null)
                modePanel.SetActive(true);
        }

        public virtual void OnDeselect()
        {
            if (modePanel != null)
                modePanel.SetActive(false);
        }
    }
}