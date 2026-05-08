using Rules;
using Rules.EmotionRules;
using TMPro;
using UnityEngine;

namespace UI.Tooltip
{
    /// <summary>
    /// Renders tooltip content into the panel's UI sections.
    /// Only the sections relevant to the current TooltipData type are active.
    ///
    /// Prefab layout (Screen Space – Overlay canvas):
    ///   Background Image → VerticalLayoutGroup + ContentSizeFitter
    ///     TitleLabel          (TMP_Text, always visible)
    ///     EmotionRow          (GameObject — HorizontalLayoutGroup, piece-only)
    ///       EmotionLabel      (TMP_Text)
    ///     WhyLabel            (TMP_Text header "Why:", piece-only)
    ///     EffectsContainer    (Transform → VerticalLayoutGroup)
    ///       TooltipEffectEntry children spawned here at runtime — each needs one TMP_Text wired
    ///     RuleSummaryLabel    (TMP_Text, emotion-rule-only)
    ///     ProgressLabel       (TMP_Text, completion-rule-only)
    /// </summary>
    public class TooltipPanel : MonoBehaviour
    {
        [Header("Shared")]
        [SerializeField] private TMP_Text titleLabel;

        [Header("Piece sections")]
        [SerializeField] private GameObject emotionRow;
        [SerializeField] private TMP_Text   emotionLabel;
        [SerializeField] private TMP_Text   whyLabel;
        [SerializeField] private Transform  effectsContainer;
        [SerializeField] private TooltipEffectEntry effectEntryPrefab;

        [Header("Emotion rule section")]
        [SerializeField] private TMP_Text ruleSummaryLabel;

        [Header("Completion rule section")]
        [SerializeField] private TMP_Text progressLabel;

        private static readonly Color HappyColor   = new Color(0.2f, 0.85f, 0.2f);
        private static readonly Color NeutralColor  = Color.white;
        private static readonly Color SadColor      = new Color(0.9f, 0.2f, 0.2f);

        // ── Public API ────────────────────────────────────────────────────────────

        public void SetData(TooltipData data)
        {
            Clear();
            switch (data)
            {
                case PieceTooltipData piece:           ShowPiece(piece);            break;
                case EmotionRuleTooltipData rule:      ShowEmotionRule(rule);       break;
                case CompletionRuleTooltipData comp:   ShowCompletionRule(comp);    break;
            }
        }

        // ── Render variants ───────────────────────────────────────────────────────

        private void ShowPiece(PieceTooltipData data)
        {
            titleLabel.text = data.Piece.Piece.sourceSO != null
                ? data.Piece.Piece.sourceSO.name
                : "Piece";

            emotionRow.SetActive(true);
            whyLabel.gameObject.SetActive(true);

            var emotion = data.EmotionState?.FinalEmotion ?? PieceEmotion.Neutral;
            emotionLabel.text  = emotion.ToString();
            emotionLabel.color = EmotionColor(emotion);

            if (data.EmotionState == null || data.EmotionState.Effects.Count == 0)
            {
                SpawnTextEntry("No rules apply");
            }
            else
            {
                foreach (var effect in data.EmotionState.Effects)
                    SpawnEffectEntry(effect);
            }
        }

        private void ShowEmotionRule(EmotionRuleTooltipData data)
        {
            titleLabel.text = data.Rule.GetDescription();

            if (ruleSummaryLabel != null)
            {
                ruleSummaryLabel.text = BuildRuleSummary(data);
                ruleSummaryLabel.gameObject.SetActive(true);
            }
        }

        private void ShowCompletionRule(CompletionRuleTooltipData data)
        {
            titleLabel.text = data.Config.rule.GetDescription();

            if (progressLabel != null)
            {
                progressLabel.text = data.Config.rule.GetProgress(data.EvaluationResult, data.State);
                progressLabel.gameObject.SetActive(true);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private void Clear()
        {
            emotionRow.SetActive(false);
            whyLabel.gameObject.SetActive(false);
            if (ruleSummaryLabel != null) ruleSummaryLabel.gameObject.SetActive(false);
            if (progressLabel    != null) progressLabel.gameObject.SetActive(false);

            for (int i = effectsContainer.childCount - 1; i >= 0; i--)
                Destroy(effectsContainer.GetChild(i).gameObject);
        }

        private void SpawnEffectEntry(EmotionEffect effect)
        {
            var entry = Instantiate(effectEntryPrefab, effectsContainer);
            entry.SetData(effect);
        }

        private void SpawnTextEntry(string text)
        {
            var entry = Instantiate(effectEntryPrefab, effectsContainer);
            entry.SetPlainText(text);
        }

        private static string BuildRuleSummary(EmotionRuleTooltipData data)
        {
            if (data.EvaluationResult == null) return "No data";

            int count = 0;
            foreach (var pieceState in data.EvaluationResult.PieceStates)
            {
                foreach (var effect in pieceState.Effects)
                {
                    if (effect.Source == data.Rule) { count++; break; }
                }
            }

            return count == 0
                ? "No pieces currently affected"
                : $"Affecting {count} piece{(count != 1 ? "s" : "")}";
        }

        private static Color EmotionColor(PieceEmotion emotion)
        {
            return emotion switch
            {
                PieceEmotion.Happy => HappyColor,
                PieceEmotion.Sad   => SadColor,
                _                  => NeutralColor,
            };
        }
    }
}
