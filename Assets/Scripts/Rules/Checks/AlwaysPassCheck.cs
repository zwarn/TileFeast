using System;
using Pieces;

namespace Rules.Checks
{
    /// <summary>
    /// Always passes. Useful for rules that only depend on the piece filter
    /// (e.g. "all Red pieces are Happy when placed").
    /// </summary>
    [Serializable]
    public class AlwaysPassCheck : EmotionCheck
    {
        public override CheckResult Evaluate(PlacedPiece piece, EmotionContext context)
            => new CheckResult(true, "placed");

        public override string GetDescription() => "placed";
    }
}
