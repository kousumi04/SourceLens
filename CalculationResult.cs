namespace SourceLensAPI.Models
{
    /// <summary>What the Calculation Engine (Phase 2) hands back to the pipeline.</summary>
    public class CalculationResult
    {
        public CalculationOperation Operation { get; set; }
        public List<NumericValue> Inputs { get; set; } = new();
        public double? CalculatedValue { get; set; }
        public NumericUnit ResultUnit { get; set; } = NumericUnit.Unknown;

        /// <summary>Set when the calculation couldn't be performed (e.g. divide by zero, too few inputs).</summary>
        public string? Error { get; set; }

        public bool Success => Error is null && CalculatedValue.HasValue;
    }

    /// <summary>
    /// A single supporting/refuting snippet returned by the teammates' Evidence/RAG
    /// module (Phase 6). Kept minimal and decoupled from their internal EF entity
    /// so this module only depends on an interface, not their implementation.
    /// </summary>
    public class EvidenceSnippet
    {
        public int EvidenceId { get; set; }
        public int SourceId { get; set; }
        public string Text { get; set; } = string.Empty;

        /// <summary>"Supports" | "Refutes" | "Neutral" — matches the existing Evidence.supportType values.</summary>
        public string SupportType { get; set; } = "Neutral";

        /// <summary>Number the RAG module itself extracted from the evidence text, if any, for direct comparison.</summary>
        public double? ExtractedNumber { get; set; }
    }

    /// <summary>
    /// Mirrors the existing ClaimAssessment table (claimId, verdict, confidence, summary)
    /// plus the numeric-specific fields this module adds. If ClaimAssessment already
    /// exists as an EF entity in the project, add these columns to it directly instead
    /// of using this class standalone.
    /// </summary>
    public class ClaimAssessment
    {
        public int Id { get; set; }
        public int ClaimId { get; set; }

        // --- existing fields (kept compatible with mockAssessments shape) ---
        public string Verdict { get; set; } = "Inconclusive"; // Supported | Refuted | Inconclusive
        public double Confidence { get; set; }
        public string Summary { get; set; } = string.Empty;

        // --- numeric verification fields (this module) ---
        public double? ClaimedValue { get; set; }
        public double? CalculatedValue { get; set; }
        public NumericUnit NumericUnit { get; set; } = NumericUnit.Unknown;
        public double? DifferencePercent { get; set; }
        public ComparisonResult ComparisonResult { get; set; } = ComparisonResult.NotComputable;
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Inconclusive;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
