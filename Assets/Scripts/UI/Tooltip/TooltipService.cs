using Board;
using Pieces;
using Rules;
using UnityEngine;
using Zenject;

namespace UI.Tooltip
{
    /// <summary>
    /// Manages the single shared tooltip panel: shows it with the right data,
    /// hides it, and keeps it positioned near the cursor each frame.
    ///
    /// Board-piece hover is handled automatically by subscribing to BoardHoverService.
    /// Rule-entry hover is driven by EmotionRuleViewEntry / CompletionRuleViewEntry
    /// calling Show() / Hide() directly.
    ///
    /// Prefab setup requirement:
    ///   - The Canvas containing this panel must be Screen Space – Overlay.
    /// </summary>
    public class TooltipService : MonoBehaviour
    {
        [SerializeField] private TooltipPanel panel;

        /// Offset from the cursor tip in screen pixels (right/down from cursor).
        [SerializeField] private Vector2 cursorOffset = new Vector2(16f, -16f);

        [Inject] private BoardHoverService _boardHoverService;

        private RectTransform _panelRT;
        private Canvas        _canvas;

        private void Awake()
        {
            if (panel == null) return;
            _panelRT = panel.GetComponent<RectTransform>();
            _canvas  = _panelRT.GetComponentInParent<Canvas>().rootCanvas;
        }

        private void OnEnable()
        {
            _boardHoverService.OnPieceHovered   += OnPieceHovered;
            _boardHoverService.OnPieceUnhovered += Hide;
        }

        private void OnDisable()
        {
            _boardHoverService.OnPieceHovered   -= OnPieceHovered;
            _boardHoverService.OnPieceUnhovered -= Hide;
        }

        private void Update()
        {
            if (panel != null && panel.gameObject.activeSelf)
                PositionNearCursor();
        }

        // ── Public API ────────────────────────────────────────────────────────────

        public void Show(TooltipData data)
        {
            if (panel == null) return;
            panel.SetData(data);
            panel.gameObject.SetActive(true);
            PositionNearCursor();
        }

        public void Hide()
        {
            if (panel == null) return;
            panel.gameObject.SetActive(false);
        }

        // ── Internal ──────────────────────────────────────────────────────────────

        private void OnPieceHovered(PlacedPiece piece, PieceEmotionState state)
        {
            Show(new PieceTooltipData { Piece = piece, EmotionState = state });
        }

        private void PositionNearCursor()
        {
            if (_panelRT == null) return;

            Vector2 screenPos = (Vector2)Input.mousePosition + cursorOffset;

            // For Screen Space – Overlay the canvas world-space position equals screen pixels,
            // so we can assign directly without any coordinate conversion.
            // rt.rect dimensions are in canvas units; multiply by scaleFactor to get screen pixels
            // for clamping so the panel stays fully inside the viewport.
            float sf = _canvas != null ? _canvas.scaleFactor : 1f;
            float w  = _panelRT.rect.width  * sf;
            float h  = _panelRT.rect.height * sf;
            screenPos.x = Mathf.Clamp(screenPos.x, 0f, Screen.width  - w);
            screenPos.y = Mathf.Clamp(screenPos.y, h,  Screen.height);

            _panelRT.position = new Vector3(screenPos.x, screenPos.y, _panelRT.position.z);
        }
    }
}
