namespace SourceLens.EvidenceRetrieval.Models;

/// <summary>
/// Data Transfer Object representing an evidence snippet retrieved for a specific claim via RAG.
/// </summary>
public class RetrievedEvidence
{
    public string EvidenceText { get; set; } = string.Empty;
    public int? PageNumber { get; set; }
    public string SectionTitle { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public int SourceId { get; set; }
    public string SourceTitle { get; set; } = string.Empty;
    public string? SourceDoi { get; set; }
    public string SourceAuthors { get; set; } = string.Empty;
    public int? PublicationYear { get; set; }
    public int ClaimId { get; set; }
    public string RetrievalMethod { get; set; } = "Vector Semantic Search";
    public string RelevanceExplanation { get; set; } = string.Empty;
}
