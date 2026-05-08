using Rules;
using Rules.EmotionRules;
using TMPro;
using UnityEngine;

namespace UI.Tooltip
{
    /// <summary>
    /// Renders one effect row inside TooltipPanel.
    ///
    /// Prefab only needs a single TMP_Text child wired to <see cref="label"/>.
    /// Content is formatted as rich text: coloured emotion + reason + muted rule source.
    /// </summary>
    public class TooltipEffectEntry : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        private const string HappyHex   = "#33DD33";
        private const string NeutralHex = "#BBBBBB";
        private const string SadHex     = "#DD3333";
        private const string SourceHex  = "#888888";

        public void SetData(EmotionEffect effect)
        {
            if (label == null) return;

            string emotionHex = effect.Emotion switch
            {
                PieceEmotion.Happy => HappyHex,
                PieceEmotion.Sad   => SadHex,
                _                  => NeutralHex,
            };

            string source = effect.Source != null
                ? $"  <color={SourceHex}><size=80%>({effect.Source.GetDescription()})</size></color>"
                : string.Empty;

            label.text = $"<color={emotionHex}>{effect.Emotion}</color>  {effect.Reason}{source}";
        }

        public void SetPlainText(string text)
        {
            if (label != null) label.text = text;
        }
    }
}
