namespace Rules.Checks
{
    /// <summary>How a neighbor count is tallied.</summary>
    public enum NeighborCountMode
    {
        /// <summary>Counts each distinct neighboring piece at most once (even if it covers multiple adjacent tiles).</summary>
        DistinctPieces,

        /// <summary>Counts each adjacent tile position occupied by a matching piece (a large piece can be counted multiple times).</summary>
        CoveredTiles
    }
}
