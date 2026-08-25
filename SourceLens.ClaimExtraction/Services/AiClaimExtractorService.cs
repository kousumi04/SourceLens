using System.Text.Json;
using SourceLens.ClaimExtraction.Interfaces;
using SourceLens.ClaimExtraction.Models;

namespace SourceLens.ClaimExtraction.Services;

public class AiClaimExtractorService : IClaimExtractor
{
    // Note: In a real implementation, you would inject an OpenAIClient or HttpClient here.
    // Example: private readonly OpenAIClient _aiClient;
    
    public async Task<List<ExtractedClaim>> ExtractAndSortClaimsAsync(string pageText, int pageNumber)
    {
        // ====================================================================================
        // TODO: Replace this simulated behavior with actual AI integration (e.g., OpenAI API)
        // 1. Send the `pageText` to the AI.
        // 2. Ask the AI to return a JSON array of claims with their categories.
        // 3. Deserialize the JSON back into a List<ExtractedClaim>.
        // ====================================================================================

        /* Example prompt to the AI:
           "You are an expert scientific parser. Read the following text and extract all factual claims. 
            For each claim, categorize it as: Background, Methodology, Finding, Hypothesis, or Conclusion.
            Return the output ONLY as a JSON array matching this format: 
            [ { 'claimText': '...', 'category': 2 } ]"
        */

        // Simulating an API call to the LLM
        await Task.Delay(500);

        var simulatedClaims = new List<ExtractedClaim>();

        // Just simulating that if we detect certain keywords, we consider it a claim.
        if (pageText.Contains("experiment", StringComparison.OrdinalIgnoreCase))
        {
            simulatedClaims.Add(new ExtractedClaim
            {
                ClaimText = "The experiment was conducted in a controlled environment.",
                Category = ClaimCategory.Methodology,
                PageNumber = pageNumber
            });
        }
        
        if (pageText.Contains("result", StringComparison.OrdinalIgnoreCase))
        {
            simulatedClaims.Add(new ExtractedClaim
            {
                ClaimText = "The results indicate a significant improvement.",
                Category = ClaimCategory.Finding,
                PageNumber = pageNumber
            });
        }

        return simulatedClaims;
    }
}
