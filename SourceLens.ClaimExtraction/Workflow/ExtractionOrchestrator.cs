using SourceLens.ClaimExtraction.Interfaces;
using SourceLens.ClaimExtraction.Models;

namespace SourceLens.ClaimExtraction.Workflow;

/// <summary>
/// This is the main orchestrator that connects the parser and the extractor.
/// Other teammates can call this class directly to process a file.
/// </summary>
public class ExtractionOrchestrator
{
    private readonly IPaperParser _paperParser;
    private readonly IClaimExtractor _claimExtractor;

    // Dependency Injection
    public ExtractionOrchestrator(IPaperParser paperParser, IClaimExtractor claimExtractor)
    {
        _paperParser = paperParser;
        _claimExtractor = claimExtractor;
    }

    /// <summary>
    /// 1. Reads the PDF.
    /// 2. Iterates through each page.
    /// 3. Extracts and sorts claims.
    /// 4. Returns the final list of all categorized claims.
    /// </summary>
    public async Task<List<ExtractedClaim>> ProcessPaperAsync(string filePath)
    {
        var allExtractedClaims = new List<ExtractedClaim>();

        // Step 1: Parse the PDF to get text by page
        var pages = await _paperParser.ExtractTextByPageAsync(filePath);

        // Step 2: Loop through each page
        foreach (var page in pages)
        {
            var pageNumber = page.Key;
            var pageText = page.Value;

            // Skip empty pages
            if (string.IsNullOrWhiteSpace(pageText))
                continue;

            // Step 3: Extract & Sort Claims from this page using AI
            var claimsFromPage = await _claimExtractor.ExtractAndSortClaimsAsync(pageText, pageNumber);
            
            allExtractedClaims.AddRange(claimsFromPage);
        }

        return allExtractedClaims;
    }
}
