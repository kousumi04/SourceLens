using SourceLensAPI.Models;

namespace SourceLensAPI.Services
{
    public class ComparisonOutcome
    {
        public double ClaimedValue { get; set; }
        public double CalculatedValue { get; set; }
        public double DifferencePercent { get; set; }   // relative difference for non-percentage units
        public double DifferencePoints { get; set; }     // absolute-point difference, meaningful for percentages/ratios
        public ComparisonResult Result { get; set; }
    }

    public interface IComparisonService
    {
        /// <summary>
        /// Compares a number stated in a claim against the number the CalculationEngine
        /// independently derived, and classifies the match.
        /// </summary>
        ComparisonOutcome Compare(double claimedValue, double calculatedValue, NumericUnit unit,
            double matchTolerancePercent = 2.0, double approximateTolerancePercent = 10.0);
    }

    /// <summary>
    /// Phase 3 — Compare Claimed vs. Calculated.
    /// Percentages/ratios are compared on absolute-point difference (a claimed "20%"
    /// vs. calculated "21%" is a 1-point gap, not a 5% relative gap) because relative
    /// comparison on small percentage values is misleading. Everything else (counts,
    /// currency, multipliers) is compared on relative % difference.
    /// </summary>
    public class ComparisonService : IComparisonService
    {
        public ComparisonOutcome Compare(double claimedValue, double calculatedValue, NumericUnit unit,
            double matchTolerancePercent = 2.0, double approximateTolerancePercent = 10.0)
        {
            var outcome = new ComparisonOutcome
            {
                ClaimedValue = claimedValue,
                CalculatedValue = calculatedValue
            };

            var absPointDiff = Math.Abs(claimedValue - calculatedValue);
            outcome.DifferencePoints = absPointDiff;

            outcome.DifferencePercent = calculatedValue == 0
                ? (claimedValue == 0 ? 0 : double.PositiveInfinity)
                : Math.Abs(claimedValue - calculatedValue) / Math.Abs(calculatedValue) * 100.0;

            // For percentage-like/ratio units, judge on absolute points; else on relative %.
            var usePoints = unit is NumericUnit.Percentage or NumericUnit.Ratio;
            var effectiveDiff = usePoints ? absPointDiff : outcome.DifferencePercent;

            outcome.Result = effectiveDiff <= matchTolerancePercent ? ComparisonResult.Match
                : effectiveDiff <= approximateTolerancePercent ? ComparisonResult.Approximate
                : ComparisonResult.Mismatch;

            return outcome;
        }
    }
}
