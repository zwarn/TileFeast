using System;
using Core;
using Pieces;
using Rules;
using UnityEngine;
using Zenject;

namespace Board
{
    /// <summary>
    /// Detects which placed piece is under the mouse each frame using the same
    /// grid-math approach as GrabTool, and fires events so systems like TooltipService
    /// can react without polling themselves.
    /// </summary>
    public class BoardHoverService : MonoBehaviour
    {
        [SerializeField] private Grid grid;

        [Inject] private BoardController _boardController;
        [Inject] private RulesController _rulesController;
        [Inject] private GameController  _gameController;

        public event Action<PlacedPiece, PieceEmotionState> OnPieceHovered;
        public event Action OnPieceUnhovered;

        private PlacedPiece _lastHovered;

        private void Update()
        {
            // Suppress hover while a piece is in hand — the cursor is busy with placement.
            // UI panels (rules, supply) manage the tooltip themselves via IPointerEnterHandler,
            // so we don't need to suppress here: hovering an empty board cell returns null from
            // GetPiece(), and rule entry OnPointerEnter/Exit calls Show/Hide directly.
            if (!_gameController.IsHandEmpty())
            {
                if (_lastHovered != null) ClearHover();
                return;
            }

            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var gridPos  = (Vector2Int)grid.WorldToCell(worldPos);
            var piece    = _boardController.GetPiece(gridPos);

            if (piece == _lastHovered) return;
            _lastHovered = piece;

            if (piece == null)
                OnPieceUnhovered?.Invoke();
            else
                OnPieceHovered?.Invoke(piece, FindEmotionState(piece));
        }

        private void ClearHover()
        {
            _lastHovered = null;
            OnPieceUnhovered?.Invoke();
        }

        private PieceEmotionState FindEmotionState(PlacedPiece piece)
        {
            var result = _rulesController.LastResult;
            if (result == null) return null;
            foreach (var s in result.PieceStates)
                if (s.Piece == piece) return s;
            return null;
        }
    }
}
