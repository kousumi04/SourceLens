using SourceLensAPI.Interfaces;
using SourceLensAPI.Models;

namespace SourceLensAPI.Services
{
    public class NumericVerificationRequest
    {
        public int ClaimId { get; set; }

        /// <summary>Full claim text. Required if Numbers is empty (triggers auto-extraction, Phase 5).</summary>
        public string? ClaimText { get; set; }

        /// <summary>Manually/structurally supplied numbers (Phase 1). If provided, extraction is skipped.</summary>
        public List<NumericValue>? Numbers { get; set; }

        /// <summary>How to derive the calculated value from Numbers (Phase 2).</summary>
        public CalculationOperation Operation { get; set; } = CalculationOperation.Identity;

        /// <summary>The number actually stated in the claim, to check the calculation against (Phase 3).</summary>
        public double ClaimedValue { get; set; }

        public NumericUnit Unit { get; set; } = NumericUnit.Unknown;

        /// <summary>Whether to pull supporting evidence via the RAG module (Phase 6).</summary>
        public bool UseEvidence { get; set; } = true;
    }

    public class NumericVerificationOutcome
    {
        public CalculationResult Calculation { get; set; } = new();
        public ComparisonOutcome? Comparison { get; set; }
        public ClaimAssessment Assessment { get; set; } = new();
        public List<EvidenceSnippet> Evidence { get; set; } = new();
    }

    public interface INumericVerificationService
    {
        Task<NumericVerificationOutcome> VerifyAsync(NumericVerificationRequest request);
    }

    /// <summary>
    /// Orchestrates the full pipeline:
    ///   Text/Claim -> Extract Numbers -> Calculate -> Compare -> Assess & Store -> Verify with Evidence/RAG
    ///
    /// Phase 1 (manual numbers) and Phase 5 (auto-extraction) both feed into Phase 2
    /// through the same NumericValue shape, so this class doesn't need to know which
    /// path produced them.
    /// </summary>
    public class NumericVerificationService : INumericVerificationService
    {
        private readonly INumberExtractionService _extractor;
        private readonly ICalculationEngine _calculator;
        private readonly IComparisonService _comparator;
        private readonly IClaimAssessmentRepository _repository;
        private readonly IEvidenceRagService _evidenceService;

        public NumericVerificationService(
            INumberExtractionService extractor,
            ICalculationEngine calculator,
            IComparisonService comparator,
            IClaimAssessmentRepository repository,
            IEvidenceRagService evidenceService)
        {
            _extractor = extractor;
            _calculator = calculator;
            _comparator = comparator;
            _repository = repository;
            _evidenceService = evidenceService;
        }

        public async Task<NumericVerificationOutcome> VerifyAsync(NumericVerificationRequest request)
        {
            var outcome = new NumericVerificationOutcome();

            // --- Phase 1 / Phase 5: get structured numbers, manual or auto-extracted ---
            var numbers = request.Numbers is { Count: > 0 }
                ? request.Numbers
                : _extractor.Extract(request.ClaimText ?? string.Empty);

            // --- Phase 2: calculate ---
            outcome.Calculation = _calculator.Calculate(request.Operation, numbers);

            // --- Phase 3: compare claimed vs calculated ---
            ComparisonResult comparisonResult = ComparisonResult.NotComputable;
            if (outcome.Calculation.Success)
            {
                var unit = request.Unit != NumericUnit.Unknown ? request.Unit : outcome.Calculation.ResultUnit;
                outcome.Comparison = _comparator.Compare(request.ClaimedValue, outcome.Calculation.CalculatedValue!.Value, unit);
                comparisonResult = outcome.Comparison.Result;
            }

            // --- Phase 6: pull supporting/refuting evidence via teammates' RAG module ---
            if (request.UseEvidence)
            {
                outcome.Evidence = await _evidenceService.GetSupportingEvidenceAsync(request.ClaimId, request.ClaimText ?? string.Empty);
            }

            // --- Phase 4: assess and store ---
            var status = DetermineStatus(comparisonResult, outcome.Evidence, outcome.Calculation.Success);
            var verdict = status switch
            {
                VerificationStatus.Verified => "Supported",
                VerificationStatus.Contradicted => "Refuted",
                _ => "Inconclusive"
            };

            outcome.Assessment = new ClaimAssessment
            {
                ClaimId = request.ClaimId,
                Verdict = verdict,
                Confidence = ComputeConfidence(comparisonResult, outcome.Evidence),
                Summary = BuildSummary(outcome.Calculation, outcome.Comparison, outcome.Evidence),
                ClaimedValue = request.ClaimedValue,
                CalculatedValue = outcome.Calculation.CalculatedValue,
                NumericUnit = request.Unit != NumericUnit.Unknown ? request.Unit : outcome.Calculation.ResultUnit,
                DifferencePercent = outcome.Comparison?.DifferencePercent,
                ComparisonResult = comparisonResult,
                VerificationStatus = status
            };

            outcome.Assessment = await _repository.SaveAsync(outcome.Assessment);
            return outcome;
        }

        private static VerificationStatus DetermineStatus(ComparisonResult comparison, List<EvidenceSnippet> evidence, bool calculationSucceeded)
        {
            if (!calculationSucceeded) return VerificationStatus.Inconclusive;

            var refutingEvidence = evidence.Any(e => e.SupportType.Equals("Refutes", StringComparison.OrdinalIgnoreCase));
            var supportingEvidence = evidence.Any(e => e.SupportType.Equals("Supports", StringComparison.OrdinalIgnoreCase));

            return comparison switch
            {
                ComparisonResult.Match => refutingEvidence ? VerificationStatus.Inconclusive : VerificationStatus.Verified,
                ComparisonResult.Approximate => supportingEvidence && !refutingEvidence
                    ? VerificationStatus.Verified
                    : VerificationStatus.Inconclusive,
                ComparisonResult.Mismatch => VerificationStatus.Contradicted,
                _ => VerificationStatus.Inconclusive
            };
        }

        private static double ComputeConfidence(ComparisonResult comparison, List<EvidenceSnippet> evidence)
        {
            double baseConfidence = comparison switch
            {
                ComparisonResult.Match => 0.9,
                ComparisonResult.Approximate => 0.65,
                ComparisonResult.Mismatch => 0.85, // confidently contradicted, not "uncertain"
                _ => 0.3
            };

            // Nudge confidence based on how much evidence agrees, without letting
            // a single snippet swing it too far.
            if (evidence.Count > 0)
            {
                var supportRatio = evidence.Count(e => e.SupportType.Equals("Supports", StringComparison.OrdinalIgnoreCase)) / (double)evidence.Count;
                baseConfidence = (baseConfidence * 0.7) + (supportRatio * 0.3);
            }

            return Math.Round(Math.Clamp(baseConfidence, 0, 1), 2);
        }

        private static string BuildSummary(CalculationResult calc, ComparisonOutcome? comparison, List<EvidenceSnippet> evidence)
        {
            if (!calc.Success)
                return $"Could not verify: {calc.Error}";

            var parts = new List<string>
            {
                $"Calculated value: {calc.CalculatedValue:0.##} (claimed: {comparison?.ClaimedValue:0.##})."
            };

            if (comparison != null)
            {
                parts.Add(comparison.Result switch
                {
                    ComparisonResult.Match => "Claim matches the independently calculated figure.",
                    ComparisonResult.Approximate => $"Claim is close to the calculated figure (diff: {comparison.DifferencePercent:0.#}%).",
                    ComparisonResult.Mismatch => $"Claim differs materially from the calculated figure (diff: {comparison.DifferencePercent:0.#}%).",
                    _ => "Comparison could not be completed."
                });
            }

            if (evidence.Count > 0)
            {
                var supports = evidence.Count(e => e.SupportType.Equals("Supports", StringComparison.OrdinalIgnoreCase));
                var refutes = evidence.Count(e => e.SupportType.Equals("Refutes", StringComparison.OrdinalIgnoreCase));
                parts.Add($"Evidence: {supports} supporting, {refutes} refuting out of {evidence.Count} snippet(s).");
            }

            return string.Join(" ", parts);
        }
    }
}
