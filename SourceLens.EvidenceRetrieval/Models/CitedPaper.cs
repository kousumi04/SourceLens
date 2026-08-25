namespace SourceLens.EvidenceRetrieval.Models;

/// <summary>
/// Represents metadata and full content of a cited scientific paper retrieved from academic sources.
/// </summary>
public class CitedPaper
{
    public string PaperId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Authors { get; set; } = string.Empty;
    public int? PublicationYear { get; set; }
    public string? Doi { get; set; }
    public string? ArxivId { get; set; }
    public string? Journal { get; set; }
    public string? Abstract { get; set; }
    public string FullText { get; set; } = string.Empty;
    public string? PdfUrl { get; set; }
    public Dictionary<string, string> Sections { get; set; } = new();
}
