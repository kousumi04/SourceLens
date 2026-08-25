namespace SourceLens.EvidenceRetrieval.Models;

/// <summary>
/// Represents a chunked section or passage of a research paper prepared for vector embeddings and RAG retrieval.
/// </summary>
public class PaperChunk
{
    public string ChunkId { get; set; } = Guid.NewGuid().ToString();
    public string PaperId { get; set; } = string.Empty;
    public string PaperTitle { get; set; } = string.Empty;
    public string? Doi { get; set; }
    public string SectionTitle { get; set; } = string.Empty;
    public int? PageNumber { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public float[]? Embedding { get; set; }
}
