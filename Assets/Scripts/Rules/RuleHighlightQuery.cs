using System.Collections.Generic;
using System.Linq;
using Pieces;
using Rules.EmotionRules;
using Rules.Filters;
using UnityEngine;

namespace Rules
{
    /// <summary>
    /// Computes HighlightData for rule/filter/check hover interactions.
    /// All methods are pure — no side effects.
    /// </summary>
    public static class RuleHighlightQuery
    {
        // Colors
        private static readonly Color FilterMatchColor  = new(0.0f, 0.85f, 0.95f, 0.75f); // cyan
        private static readonly Color FilterMissColor   = new(0.3f, 0.3f,  0.3f,  0.55f); // dark grey
        private static readonly Color HappyEffectColor  = new(0.2f, 0.9f,  0.2f,  0.75f); // green
        private static readonly Color SadEffectColor    = new(0.9f, 0.2f,  0.2f,  0.75f); // red
        private static readonly Color NoEffectColor     = new(0.4f, 0.4f,  0.4f,  0.45f); // dim grey

        /// <summary>
        /// Highlights filter-matching pieces (cyan) and non-matching placed pieces (grey).
        /// </summary>
        public static HighlightData GetFilterHighlight(PieceFilter filter, EmotionContext context)
        {
            if (filter == null || context?.State?.PlacedPieces == null)
                return HighlightData.Empty();

            var matchPositions   = new List<Vector2Int>();
            var noMatchPositions = new List<Vector2Int>();

            foreach (var placed in context.State.PlacedPieces)
            {
                if (!placed.Piece.hasEmotions) continue;
                var positions = placed.GetTilePosition();
                if (filter.Matches(placed, context))
                    matchPositions.AddRange(positions);
                else
                    noMatchPositions.AddRange(positions);
            }

            var groups = new List<HighlightGroup>();
            if (matchPositions.Count > 0)   groups.Add(new HighlightGroup(FilterMatchColor, matchPositions));
            if (noMatchPositions.Count > 0) groups.Add(new HighlightGroup(FilterMissColor,  noMatchPositions));
            return new HighlightData(groups);
        }

        /// <summary>
        /// Highlights filter-matching pieces (cyan) plus check-specific context (e.g., neighbors, groups).
        /// </summary>
        public static HighlightData GetCheckContextHighlight(EmotionRule rule, EmotionContext context)
        {
            if (rule?.filter == null || context?.State?.PlacedPieces == null)
                return HighlightData.Empty();

            var matched = context.State.PlacedPieces
                .Where(p => p.Piece.hasEmotions && rule.filter.Matches(p, context))
                .ToList();

            var matchPositions = matched.SelectMany(p => p.GetTilePosition()).ToList();
            var groups         = new List<HighlightGroup>();

            if (matchPositions.Count > 0)
                groups.Add(new HighlightGroup(FilterMatchColor, matchPositions));

            if (rule.check != null)
                groups.AddRange(rule.check.GetContextHighlight(matched, context));

            return new HighlightData(groups);
        }

        /// <summary>
        /// Highlights pieces by the emotion effect this specific rule gave them.
        /// Pieces where the rule produced no effect are shown in dim grey.
        /// </summary>
        public static HighlightData GetRuleEffectHighlight(EmotionRule rule, EmotionEvaluationResult lastResult)
        {
            if (rule == null || lastResult == null)
                return HighlightData.Empty();

            var happyPositions   = new List<Vector2Int>();
            var sadPositions     = new List<Vector2Int>();
            var noEffectPositions = new List<Vector2Int>();

            foreach (var state in lastResult.PieceStates)
            {
                var effect = state.Effects.FirstOrDefault(e => ReferenceEquals(e.Source, rule));
                var positions = state.Piece.GetTilePosition();
                if (effect == null)
                    noEffectPositions.AddRange(positions);
                else if (effect.Emotion == PieceEmotion.Happy)
                    happyPositions.AddRange(positions);
                else if (effect.Emotion == PieceEmotion.Sad)
                    sadPositions.AddRange(positions);
            }

            var groups = new List<HighlightGroup>();
            if (happyPositions.Count > 0)    groups.Add(new HighlightGroup(HappyEffectColor, happyPositions));
            if (sadPositions.Count > 0)      groups.Add(new HighlightGroup(SadEffectColor,   sadPositions));
            if (noEffectPositions.Count > 0) groups.Add(new HighlightGroup(NoEffectColor,    noEffectPositions));
            return new HighlightData(groups);
        }

        /// <summary>
        /// Highlights all placed pieces that have emotions (used by CompletionRule hover).
        /// </summary>
        public static HighlightData GetAllEmotionPiecesHighlight(EmotionEvaluationResult lastResult)
        {
            if (lastResult == null) return HighlightData.Empty();

            var groups = new List<HighlightGroup>();
            var happy   = new List<Vector2Int>();
            var neutral = new List<Vector2Int>();
            var sad     = new List<Vector2Int>();

            foreach (var state in lastResult.PieceStates)
            {
                var positions = state.Piece.GetTilePosition();
                switch (state.FinalEmotion)
                {
                    case PieceEmotion.Happy:   happy.AddRange(positions);   break;
                    case PieceEmotion.Neutral:  neutral.AddRange(positions); break;
                    case PieceEmotion.Sad:      sad.AddRange(positions);     break;
                }
            }

            if (happy.Count > 0)   groups.Add(new HighlightGroup(HappyEffectColor, happy));
            if (neutral.Count > 0) groups.Add(new HighlightGroup(NoEffectColor,    neutral));
            if (sad.Count > 0)     groups.Add(new HighlightGroup(SadEffectColor,   sad));
            return new HighlightData(groups);
        }
    }
}
