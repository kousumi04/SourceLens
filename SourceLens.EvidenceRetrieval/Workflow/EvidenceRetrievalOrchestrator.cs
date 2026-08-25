using SourceLens.EvidenceRetrieval.Interfaces;
using SourceLens.EvidenceRetrieval.Models;

namespace SourceLens.EvidenceRetrieval.Workflow;

/// <summary>
/// Main orchestrator that connects the paper fetcher, chunker, and RAG retriever
/// to retrieve cited evidence snippets for given claims.
/// </summary>
public class EvidenceRetrievalOrchestrator
{
    private readonly ICitedPaperFetcher _paperFetcher;
    private readonly ITextChunker _textChunker;
    private readonly IEvidenceRetriever _evidenceRetriever;

    public EvidenceRetrievalOrchestrator(
        ICitedPaperFetcher paperFetcher,
        ITextChunker textChunker,
        IEvidenceRetriever evidenceRetriever)
    {
        _paperFetcher = paperFetcher;
        _textChunker = textChunker;
        _evidenceRetriever = evidenceRetriever;
    }

    /// <summary>
    /// Executes the full RAG pipeline for a claim and cited paper query / DOI:
    /// 1. Searches/fetches the cited paper text.
    /// 2. Chunks the cited paper into structured passages.
    /// 3. Runs vector similarity RAG search to surface top supporting/refuting evidence.
    /// </summary>
    /// <param name="query">The search query including claim text, DOI/title, and thresholds.</param>
    /// <returns>Ranked list of retrieved evidence passages.</returns>
    public async Task<List<RetrievedEvidence>> ProcessAndRetrieveEvidenceAsync(EvidenceSearchQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.ClaimText))
            return new List<RetrievedEvidence>();

        // Step 1: Fetch cited paper (by DOI if provided, otherwise by title / keywords)
        CitedPaper? citedPaper = null;

        if (!string.IsNullOrWhiteSpace(query.CitedPaperDoi))
        {
            citedPaper = await _paperFetcher.FetchPaperByDoiAsync(query.CitedPaperDoi);
        }

        if (citedPaper == null && !string.IsNullOrWhiteSpace(query.CitedPaperTitle))
        {
            var searchResults = await _paperFetcher.SearchAndFetchPapersAsync(query.CitedPaperTitle, maxResults: 1);
            citedPaper = searchResults.FirstOrDefault();
        }

        // If no specific paper was found or specified, search using claim keywords
        if (citedPaper == null)
        {
            var searchResults = await _paperFetcher.SearchAndFetchPapersAsync(query.ClaimText, maxResults: 1);
            citedPaper = searchResults.FirstOrDefault();
        }

        if (citedPaper == null)
        {
            return new List<RetrievedEvidence>();
        }

        // Step 2: Chunk the cited paper
        var chunks = _textChunker.ChunkPaper(citedPaper);
        if (chunks.Count == 0)
        {
            return new List<RetrievedEvidence>();
        }

        // Step 3: Run RAG vector search to find evidence matching the claim
        var retrievedEvidence = await _evidenceRetriever.RetrieveEvidenceAsync(
            query.ClaimText,
            chunks,
            topK: query.TopK,
            minSimilarityThreshold: query.MinSimilarityThreshold
        );

        // Step 4: Attach context metadata (ClaimId, SourceId, Authors, Year)
        foreach (var evidence in retrievedEvidence)
        {
            evidence.ClaimId = query.ClaimId;
            evidence.SourceId = query.SourceId;
            evidence.SourceAuthors = citedPaper.Authors;
            evidence.PublicationYear = citedPaper.PublicationYear;
            if (string.IsNullOrEmpty(evidence.SourceDoi))
            {
                evidence.SourceDoi = citedPaper.Doi;
            }
            if (string.IsNullOrEmpty(evidence.SourceTitle))
            {
                evidence.SourceTitle = citedPaper.Title;
            }
        }

        return retrievedEvidence;
    }
}
