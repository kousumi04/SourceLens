using SourceLensAPI.Models;

namespace SourceLensAPI.Services
{
    public interface ICalculationEngine
    {
        /// <summary>Runs the given operation over the input numbers and returns the independently-calculated value.</summary>
        CalculationResult Calculate(CalculationOperation operation, List<NumericValue> inputs);
    }

    /// <summary>
    /// Phase 2 — Numeric Calculation Engine.
    /// Takes structured numbers (whether typed manually in Phase 1 or pulled out
    /// automatically in Phase 5) and independently derives the value a claim is
    /// making a statement about, so it can be checked against what was claimed.
    /// </summary>
    public class CalculationEngine : ICalculationEngine
    {
        public CalculationResult Calculate(CalculationOperation operation, List<NumericValue> inputs)
        {
            var result = new CalculationResult { Operation = operation, Inputs = inputs };

            if (inputs == null || inputs.Count == 0)
            {
                result.Error = "No input numbers provided.";
                return result;
            }

            try
            {
                switch (operation)
                {
                    case CalculationOperation.Identity:
                        result.CalculatedValue = inputs[0].Value;
                        result.ResultUnit = inputs[0].Unit;
                        break;

                    case CalculationOperation.Sum:
                        result.CalculatedValue = inputs.Sum(i => i.Value);
                        result.ResultUnit = inputs[0].Unit;
                        break;

                    case CalculationOperation.Average:
                        result.CalculatedValue = inputs.Average(i => i.Value);
                        result.ResultUnit = inputs[0].Unit;
                        break;

                    case CalculationOperation.Difference:
                        RequireAtLeast(inputs, 2, "Difference");
                        result.CalculatedValue = inputs[0].Value - inputs[1].Value;
                        result.ResultUnit = inputs[0].Unit;
                        break;

                    case CalculationOperation.PercentageChange:
                        RequireAtLeast(inputs, 2, "PercentageChange");
                        var oldVal = inputs[0].Value;
                        var newVal = inputs[1].Value;
                        if (oldVal == 0)
                        {
                            result.Error = "Cannot compute percentage change from a baseline of zero.";
                            break;
                        }
                        result.CalculatedValue = (newVal - oldVal) / Math.Abs(oldVal) * 100.0;
                        result.ResultUnit = NumericUnit.Percentage;
                        break;

                    case CalculationOperation.Ratio:
                        RequireAtLeast(inputs, 2, "Ratio");
                        if (inputs[1].Value == 0)
                        {
                            result.Error = "Cannot compute ratio with a zero denominator.";
                            break;
                        }
                        result.CalculatedValue = inputs[0].Value / inputs[1].Value;
                        result.ResultUnit = NumericUnit.Ratio;
                        break;

                    default:
                        result.Error = $"Unsupported operation: {operation}";
                        break;
                }
            }
            catch (ArgumentException ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        private static void RequireAtLeast(List<NumericValue> inputs, int count, string opName)
        {
            if (inputs.Count < count)
                throw new ArgumentException($"{opName} requires at least {count} input numbers, got {inputs.Count}.");
        }
    }
}
