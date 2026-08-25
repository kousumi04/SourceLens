namespace SourceLens.ClaimExtraction.Models;

/// <summary>
/// Data Transfer Object representing a claim extracted from a document.
/// </summary>
public class ExtractedClaim
{
    public string ClaimText { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public ClaimCategory Category { get; set; }
}
