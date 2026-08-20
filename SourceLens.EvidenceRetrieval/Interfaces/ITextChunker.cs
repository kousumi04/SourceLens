using SourceLens.EvidenceRetrieval.Models;

namespace SourceLens.EvidenceRetrieval.Interfaces;

/// <summary>
/// Service interface for chunking cited research papers into overlapping, semantically coherent passages.
/// </summary>
public interface ITextChunker
{
    /// <summary>
    /// Chunks a cited paper's sections, abstract, or full-text into search-ready passages.
    /// </summary>
    /// <param name="paper">The cited paper object.</param>
    /// <param name="maxWordsPerChunk">Target word length for each chunk.</param>
    /// <param name="overlapWords">Number of overlapping words between consecutive chunks.</param>
    /// <returns>A list of chunk objects with section and position metadata.</returns>
    List<PaperChunk> ChunkPaper(CitedPaper paper, int maxWordsPerChunk = 150, int overlapWords = 30);
}
