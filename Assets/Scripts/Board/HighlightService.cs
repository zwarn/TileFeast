using System;
using Core;
using Pieces;
using Rules;
using Rules.EmotionRules;
using Rules.Filters;
using UnityEngine;
using Zenject;
using Zones;

namespace Board
{
    /// <summary>
    /// Central coordinator for all rule-driven highlighting.
    /// Handles board tile highlights (via HighlightController) and fires events
    /// so supply entries can tint themselves accordingly.
    /// </summary>
    public class HighlightService : MonoBehaviour
    {
        [Inject] private GameController      _gameController;
        [Inject] private HighlightController _highlightController;
        [Inject] private RulesController     _rulesController;
        [Inject] private BoardController     _boardController;
        [Inject] private ZoneController      _zoneController;

        /// <summary>
        /// Fired whenever a hover changes supply highlighting.
        /// Predicate returns true for pieces that should be highlighted (cyan),
        /// false for pieces that should be dimmed.
        /// </summary>
        public event Action<Predicate<Piece>> OnSupplyHighlightChanged;

        /// <summary>Fired when all hover highlighting should be cleared.</summary>
        public event Action OnSupplyHighlightCleared;

        private void OnEnable()
        {
            _gameController.OnBoardChanged += OnBoardChanged;
        }

        private void OnDisable()
        {
            _gameController.OnBoardChanged -= OnBoardChanged;
        }

        private void OnBoardChanged()
        {
            // Board state changed — refresh any active highlight so positions stay current.
            // The UI hover handlers re-call their highlight method when they detect a change,
            // so clearing here is the safe default.
            ClearAll();
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Highlight pieces matching the filter (cyan) and non-matching pieces (grey).
        /// Also tints matching supply pieces.
        /// </summary>
        public void HighlightFilter(PieceFilter filter)
        {
            if (filter == null) { ClearAll(); return; }
            var context = BuildContext();
            _highlightController.SetHighlight(RuleHighlightQuery.GetFilterHighlight(filter, context));
            OnSupplyHighlightChanged?.Invoke(piece => filter.MatchesSupplyPiece(piece));
        }

        /// <summary>
        /// Highlight filter-matching pieces (cyan) plus check-context positions
        /// (neighbors / group members, shown in yellow or check-specific colors).
        /// </summary>
        public void HighlightCheckContext(EmotionRule rule)
        {
            if (rule == null) { ClearAll(); return; }
            var context = BuildContext();
            _highlightController.SetHighlight(RuleHighlightQuery.GetCheckContextHighlight(rule, context));
            OnSupplyHighlightChanged?.Invoke(piece => rule.filter?.MatchesSupplyPiece(piece) ?? false);
        }

        /// <summary>
        /// Highlight pieces by the emotion effect this rule gave them
        /// (green = happy, red = sad, grey = no effect from this rule).
        /// </summary>
        public void HighlightRuleEffects(EmotionRule rule)
        {
            if (rule == null) { ClearAll(); return; }
            _highlightController.SetHighlight(
                RuleHighlightQuery.GetRuleEffectHighlight(rule, _rulesController.LastResult));
            OnSupplyHighlightChanged?.Invoke(piece => rule.filter?.MatchesSupplyPiece(piece) ?? false);
        }

        /// <summary>
        /// Highlight all placed pieces by their current final emotion.
        /// Used for CompletionRule hover.
        /// </summary>
        public void HighlightAllEmotions()
        {
            _highlightController.SetHighlight(
                RuleHighlightQuery.GetAllEmotionPiecesHighlight(_rulesController.LastResult));
            OnSupplyHighlightCleared?.Invoke();
        }

        /// <summary>Clears all board and supply highlighting.</summary>
        public void ClearAll()
        {
            _highlightController.ResetHighlight();
            OnSupplyHighlightCleared?.Invoke();
        }

        // ── Internal ──────────────────────────────────────────────────────────────

        private EmotionContext BuildContext()
        {
            var state     = _gameController.CurrentState;
            var tileDict  = _boardController.GetPieceByPosition();
            var tileArray = RulesHelper.ConvertTiles(tileDict, state.GridSize.x, state.GridSize.y);
            return new EmotionContext(state, tileArray, _zoneController.Zones, _rulesController.LastResult);
        }
    }
}
