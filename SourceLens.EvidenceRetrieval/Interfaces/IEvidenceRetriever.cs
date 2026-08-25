using SourceLens.EvidenceRetrieval.Models;

namespace SourceLens.EvidenceRetrieval.Interfaces;

/// <summary>
/// Service interface for RAG vector retrieval: searches indexed paper chunks to find the most relevant evidence for a given claim.
/// </summary>
public interface IEvidenceRetriever
{
    /// <summary>
    /// Computes semantic similarity and hybrid relevance between a claim and paper chunks, returning the top-K evidence passages.
    /// </summary>
    /// <param name="claimText">The text of the scientific claim to verify.</param>
    /// <param name="chunks">The candidate paper chunks containing embedded vectors or raw text.</param>
    /// <param name="topK">The maximum number of evidence snippets to return.</param>
    /// <param name="minSimilarityThreshold">The minimum cosine similarity score required (0.0 to 1.0).</param>
    /// <returns>Ranked list of retrieved evidence snippets.</returns>
    Task<List<RetrievedEvidence>> RetrieveEvidenceAsync(
        string claimText, 
        List<PaperChunk> chunks, 
        int topK = 3, 
        float minSimilarityThreshold = 0.5f);
}
