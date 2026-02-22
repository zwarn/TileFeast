using Tools;
using UnityEngine;
using Zenject;

namespace Core
{
    public class EditorModeController : MonoBehaviour
    {
        [SerializeField] private GameObject toolSelectionPanel;

        [Inject] private ToolController _toolController;

        private bool _editorModeEnabled = true;

        private void Start()
        {
            SetEditorMode(false);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                ToggleEditorMode();
            }
#endif
        }

        private void ToggleEditorMode()
        {
            SetEditorMode(!_editorModeEnabled);
        }

        private void SetEditorMode(bool enabled)
        {
            _editorModeEnabled = enabled;
            toolSelectionPanel.SetActive(enabled);

            if (!enabled)
            {
                _toolController.ChangeTool(ToolType.GrabTool);
            }
        }
    }
}