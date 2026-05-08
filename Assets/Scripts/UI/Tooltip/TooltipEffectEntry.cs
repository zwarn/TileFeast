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

        public void SetData(EmotionEffect effect)
        {
            if (label == null) return;

            string emotionHex = effect.Emotion switch
            {
                PieceEmotion.Happy => HappyHex,
                PieceEmotion.Sad   => SadHex,
                _                  => NeutralHex,
            };

            label.text = $"<color={emotionHex}>{effect.Emotion}</color>  {effect.Reason}";
        }

        public void SetPlainText(string text)
        {
            if (label != null) label.text = text;
        }
    }
}
