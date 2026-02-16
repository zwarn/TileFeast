using UnityEngine;

namespace Tools
{
    public abstract class ModeTool : ToolBase
    {
        public override void OnSelect()
        {
            gameObject.SetActive(true);
        }

        public override void OnDeselect()
        {
            gameObject.SetActive(false);
        }
    }
}