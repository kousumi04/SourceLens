namespace SourceLens.EvidenceRetrieval.Models;

/// <summary>
/// Parameters for searching and retrieving evidence for a claim against cited academic sources.
/// </summary>
public class EvidenceSearchQuery
{
    public int ClaimId { get; set; }
    public string ClaimText { get; set; } = string.Empty;
    public string? CitedPaperTitle { get; set; }
    public string? CitedPaperDoi { get; set; }
    public int SourceId { get; set; }
    public int TopK { get; set; } = 3;
    public float MinSimilarityThreshold { get; set; } = 0.55f;
}
