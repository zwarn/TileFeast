using System.Collections.Generic;
using Board;
using Pieces;
using Rules;
using Rules.EmotionRules;
using UnityEngine;
using Zenject;

namespace UI.Tooltip
{
    /// <summary>
    /// Manages the single shared tooltip panel: shows it with the right data after a hover
    /// delay, hides it immediately, and keeps it positioned near the cursor each frame.
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

        [Tooltip("Seconds the cursor must remain over an element before the tooltip appears.")]
        [SerializeField] private float hoverDelay = 2f;

        /// Offset from the cursor tip in screen pixels (positive x = right, negative y = below).
        [SerializeField] private Vector2 cursorOffset = new Vector2(16f, -16f);

        [Inject] private BoardHoverService _boardHoverService;
        [Inject] private HighlightService  _highlightService;

        private RectTransform _panelRT;
        private Canvas        _canvas;

        private TooltipData _pending;
        private float       _pendingTime = float.MaxValue;

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
            if (panel == null) return;

            if (panel.gameObject.activeSelf)
                PositionNearCursor();

            // Commit the pending tooltip once the hover delay has elapsed.
            if (_pending != null && !panel.gameObject.activeSelf
                && Time.time - _pendingTime >= hoverDelay)
            {
                panel.SetData(_pending);
                panel.gameObject.SetActive(true);
                PositionNearCursor();
                HighlightSourceRules(_pending);
            }
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Schedule a tooltip to appear after the hover delay.
        /// Any previously pending or visible tooltip is dismissed immediately
        /// so the timer always resets when the hover target changes.
        /// </summary>
        public void Show(TooltipData data)
        {
            if (panel == null) return;
            // Hide whatever was visible / scheduled so the delay restarts cleanly.
            panel.gameObject.SetActive(false);
            _highlightService?.ClearAll();
            _pending     = data;
            _pendingTime = Time.time;
        }

        /// <summary>Cancel any pending tooltip and hide the panel immediately.</summary>
        public void Hide()
        {
            _pending     = null;
            _pendingTime = float.MaxValue;
            if (panel != null) panel.gameObject.SetActive(false);
            _highlightService?.ClearAll();
        }

        // ── Internal ──────────────────────────────────────────────────────────────

        private void OnPieceHovered(PlacedPiece piece, PieceEmotionState state)
        {
            Show(new PieceTooltipData { Piece = piece, EmotionState = state });
        }

        private void HighlightSourceRules(TooltipData data)
        {
            if (_highlightService == null) return;
            if (data is not PieceTooltipData pieceData
                || pieceData.EmotionState == null
                || pieceData.EmotionState.Effects.Count == 0)
                return;

            var rules = new HashSet<EmotionRule>();
            foreach (var effect in pieceData.EmotionState.Effects)
                if (effect.Source != null) rules.Add(effect.Source);

            if (rules.Count > 0)
                _highlightService.HighlightRulesForPiece(rules);
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
