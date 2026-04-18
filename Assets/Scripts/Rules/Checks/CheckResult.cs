namespace Rules.Checks
{
    /// <summary>
    /// Outcome of an EmotionCheck evaluation. DetailReason is an optional short phrase
    /// used by conclusions to enrich the effect's Reason string
    /// (e.g. "in a Red group of size 5").
    /// </summary>
    public class CheckResult
    {
        public bool Passed { get; }
        public string DetailReason { get; }

        public CheckResult(bool passed, string detailReason = null)
        {
            Passed = passed;
            DetailReason = detailReason;
        }
    }
}
