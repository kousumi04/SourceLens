using SourceLens.ClaimExtraction.Models;

namespace SourceLens.ClaimExtraction.Interfaces;

public interface IClaimExtractor
{
    /// <summary>
    /// Analyzes a page of text, extracts scientific claims, and sorts them by category.
    /// </summary>
    /// <param name="pageText">The raw text of the page.</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <returns>A list of extracted claims.</returns>
    Task<List<ExtractedClaim>> ExtractAndSortClaimsAsync(string pageText, int pageNumber);
}
