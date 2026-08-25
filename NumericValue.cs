namespace SourceLensAPI.Models
{
    /// <summary>
    /// The kind of quantity a number represents. Drives how the
    /// CalculationEngine and ComparisonService treat it (e.g. percentages
    /// compare on absolute-point difference, currency/count compare on
    /// relative % difference).
    /// </summary>
    public enum NumericUnit
    {
        Count,
        Percentage,
        Currency,
        Ratio,
        Multiplier,   // e.g. "2x faster"
        Date,
        Unknown
    }

    /// <summary>
    /// A single number, whether it was typed in manually (Phase 1) or pulled
    /// out of free text automatically (Phase 5). Both paths produce this same
    /// shape so the rest of the pipeline doesn't care where a number came from.
    /// </summary>
    public class NumericValue
    {
        /// <summary>The numeric value itself, normalized (e.g. "20%" -> 20).</summary>
        public double Value { get; set; }

        public NumericUnit Unit { get; set; } = NumericUnit.Unknown;

        /// <summary>Exact substring the number was parsed from (for auditing/highlighting).</summary>
        public string? RawText { get; set; }

        /// <summary>Character offset in the source text where RawText starts (-1 if manually entered).</summary>
        public int SourceOffset { get; set; } = -1;

        /// <summary>Free-text label describing what this number measures, e.g. "accuracy improvement".</summary>
        public string? Label { get; set; }

        public NumericValue() { }

        public NumericValue(double value, NumericUnit unit, string? label = null, string? rawText = null, int sourceOffset = -1)
        {
            Value = value;
            Unit = unit;
            Label = label;
            RawText = rawText;
            SourceOffset = sourceOffset;
        }
    }

    /// <summary>The arithmetic operation the CalculationEngine should perform on a set of NumericValues.</summary>
    public enum CalculationOperation
    {
        Sum,
        Average,
        Difference,
        PercentageChange,   // (new - old) / old * 100
        Ratio,              // a / b
        Identity            // pass a single number through unchanged (no calculation needed)
    }

    /// <summary>Result of comparing a claimed number against an independently calculated one.</summary>
    public enum ComparisonResult
    {
        Match,          // within tight tolerance
        Approximate,    // within loose tolerance but not tight
        Mismatch,       // outside tolerance
        NotComputable   // insufficient data to calculate/compare
    }

    /// <summary>Overall status stored on the ClaimAssessment for this numeric check.</summary>
    public enum VerificationStatus
    {
        Verified,       // claimed number matches calculation (and evidence, if used)
        Contradicted,   // claimed number conflicts with calculation or evidence
        Inconclusive,   // not enough evidence/data to decide
        PendingEvidence // calculation done, waiting on Evidence/RAG lookup
    }
}
