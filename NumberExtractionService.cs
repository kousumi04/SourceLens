using System.Globalization;
using System.Text.RegularExpressions;
using SourceLensAPI.Models;

namespace SourceLensAPI.Services
{
    public interface INumberExtractionService
    {
        /// <summary>Pulls every number, percentage, currency amount, ratio, and date out of free text.</summary>
        List<NumericValue> Extract(string text);
    }

    /// <summary>
    /// Phase 5 — Automatic Number Extraction.
    /// Regex-based extractor so claims like "improves accuracy by 20% over static
    /// retrieval" or "reduces manual verification time by half" don't need to be
    /// re-typed by hand. Deterministic and dependency-free; swap in an NLP model
    /// later without changing the INumberExtractionService contract.
    /// </summary>
    public class NumberExtractionService : INumberExtractionService
    {
        // 12,345.67  |  1234  |  3.5
        private static readonly Regex NumberPattern = new(
            @"(?<![\w])(\d{1,3}(?:,\d{3})*(?:\.\d+)?|\d+(?:\.\d+)?)(?!\w)",
            RegexOptions.Compiled);

        private static readonly Regex PercentPattern = new(
            @"(\d+(?:\.\d+)?)\s*%|\bpercent(?:age)?\s*(?:of|:)?\s*(\d+(?:\.\d+)?)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex CurrencyPattern = new(
            @"[$£€]\s?(\d{1,3}(?:,\d{3})*(?:\.\d+)?)\s*(k|m|billion|million|thousand)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RatioPattern = new(
            @"(\d+(?:\.\d+)?)\s*[:/]\s*(\d+(?:\.\d+)?)|\br\s*=\s*(\d+(?:\.\d+)?)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex MultiplierPattern = new(
            @"(\d+(?:\.\d+)?)\s*[xX](?:\s|$)",
            RegexOptions.Compiled);

        private static readonly Regex DatePattern = new(
            @"\b(\d{4})-(\d{2})-(\d{2})\b|\b(19|20)\d{2}\b",
            RegexOptions.Compiled);

        // words that turn a following number into a "reduced by / increased by" style
        // magnitude — used only to tag a Label, not to change the parsed value.
        private static readonly (string phrase, string label)[] ContextPhrases =
        {
            ("less than",     "upper bound"),
            ("more than",     "lower bound"),
            ("at least",      "lower bound"),
            ("up to",         "upper bound"),
            ("by",            "delta"),
            ("increase",      "increase"),
            ("decrease",      "decrease"),
            ("reduce",        "reduction"),
            ("improve",       "improvement"),
        };

        public List<NumericValue> Extract(string text)
        {
            var results = new List<NumericValue>();
            if (string.IsNullOrWhiteSpace(text)) return results;

            var claimed = new HashSet<int>(); // character offsets already consumed by a more specific pattern

            // 1. Currency — most specific, claim first
            foreach (Match m in CurrencyPattern.Matches(text))
            {
                if (!double.TryParse(m.Groups[1].Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                    continue;

                val = ApplyScale(val, m.Groups[2].Success ? m.Groups[2].Value : null);
                results.Add(new NumericValue(val, NumericUnit.Currency, InferLabel(text, m.Index), m.Value, m.Index));
                MarkRange(claimed, m.Index, m.Length);
            }

            // 2. Percentages
            foreach (Match m in PercentPattern.Matches(text))
            {
                var group = m.Groups[1].Success ? m.Groups[1] : m.Groups[2];
                if (!group.Success || !double.TryParse(group.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                    continue;

                results.Add(new NumericValue(val, NumericUnit.Percentage, InferLabel(text, m.Index), m.Value, m.Index));
                MarkRange(claimed, m.Index, m.Length);
            }

            // 3. Multipliers ("2x faster")
            foreach (Match m in MultiplierPattern.Matches(text))
            {
                if (!double.TryParse(m.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                    continue;

                results.Add(new NumericValue(val, NumericUnit.Multiplier, InferLabel(text, m.Index), m.Value, m.Index));
                MarkRange(claimed, m.Index, m.Length);
            }

            // 4. Ratios / correlations ("r = 0.86", "3:1")
            foreach (Match m in RatioPattern.Matches(text))
            {
                var raw = m.Groups[3].Success ? m.Groups[3].Value
                        : m.Groups[1].Success ? m.Groups[1].Value
                        : null;
                if (raw != null && double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                {
                    results.Add(new NumericValue(val, NumericUnit.Ratio, InferLabel(text, m.Index), m.Value, m.Index));
                    MarkRange(claimed, m.Index, m.Length);
                }
            }

            // 5. Dates (kept separate so they're never mistaken for a plain count, e.g. "in 2024")
            foreach (Match m in DatePattern.Matches(text))
            {
                results.Add(new NumericValue(0, NumericUnit.Date, InferLabel(text, m.Index), m.Value, m.Index));
                MarkRange(claimed, m.Index, m.Length);
            }

            // 6. Everything else plain: bare counts, not already claimed by a pattern above
            foreach (Match m in NumberPattern.Matches(text))
            {
                if (IsClaimed(claimed, m.Index)) continue;
                if (!double.TryParse(m.Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                    continue;

                results.Add(new NumericValue(val, NumericUnit.Count, InferLabel(text, m.Index), m.Value, m.Index));
            }

            return results.OrderBy(r => r.SourceOffset).ToList();
        }

        private static double ApplyScale(double value, string? scaleWord)
        {
            if (string.IsNullOrEmpty(scaleWord)) return value;
            return scaleWord.ToLowerInvariant() switch
            {
                "k" or "thousand" => value * 1_000,
                "m" or "million" => value * 1_000_000,
                "billion" => value * 1_000_000_000,
                _ => value
            };
        }

        /// <summary>Looks a short window before the match for a context phrase to label what the number means.</summary>
        private static string? InferLabel(string text, int matchIndex)
        {
            var start = Math.Max(0, matchIndex - 40);
            var window = text.Substring(start, matchIndex - start).ToLowerInvariant();

            foreach (var (phrase, label) in ContextPhrases)
            {
                if (window.Contains(phrase)) return label;
            }
            return null;
        }

        private static void MarkRange(HashSet<int> claimed, int start, int length)
        {
            for (int i = start; i < start + length; i++) claimed.Add(i);
        }

        private static bool IsClaimed(HashSet<int> claimed, int index) => claimed.Contains(index);
    }
}
