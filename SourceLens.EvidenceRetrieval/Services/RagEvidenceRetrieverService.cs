using SourceLens.EvidenceRetrieval.Interfaces;
using SourceLens.EvidenceRetrieval.Models;

namespace SourceLens.EvidenceRetrieval.Services;

/// <summary>
/// Implements RAG vector similarity search and hybrid relevance scoring to retrieve top evidence passages.
/// </summary>
public class RagEvidenceRetrieverService : IEvidenceRetriever
{
    private readonly IEmbeddingService _embeddingService;

    public RagEvidenceRetrieverService(IEmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
    }

    public async Task<List<RetrievedEvidence>> RetrieveEvidenceAsync(
        string claimText, 
        List<PaperChunk> chunks, 
        int topK = 3, 
        float minSimilarityThreshold = 0.45f)
    {
        if (string.IsNullOrWhiteSpace(claimText) || chunks == null || chunks.Count == 0)
            return new List<RetrievedEvidence>();

        // 1. Generate embedding for the claim
        var claimEmbedding = await _embeddingService.GenerateEmbeddingAsync(claimText);

        // 2. Ensure all chunks have embeddings
        var missingChunks = chunks.Where(c => c.Embedding == null || c.Embedding.Length == 0).ToList();
        if (missingChunks.Count > 0)
        {
            var chunkEmbeddings = await _embeddingService.GenerateEmbeddingsAsync(missingChunks.Select(c => c.Content));
            for (int i = 0; i < missingChunks.Count; i++)
            {
                missingChunks[i].Embedding = chunkEmbeddings[i];
            }
        }

        // 3. Compute hybrid scores
        var scoredCandidates = new List<(PaperChunk Chunk, double Score, double CosineSim)>();
        var claimTokens = ExtractSignificantTokens(claimText);

        foreach (var chunk in chunks)
        {
            if (chunk.Embedding == null)
                continue;

            double cosineSim = ComputeCosineSimilarity(claimEmbedding, chunk.Embedding);
            double keywordScore = ComputeKeywordOverlapScore(claimTokens, chunk.Content);

            // Hybrid score: 70% semantic vector similarity + 30% keyword overlap
            double hybridScore = (cosineSim * 0.70) + (keywordScore * 0.30);

            if (hybridScore >= minSimilarityThreshold)
            {
                scoredCandidates.Add((chunk, hybridScore, cosineSim));
            }
        }

        // 4. Rank candidates and take Top-K
        var topResults = scoredCandidates
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(match => new RetrievedEvidence
            {
                EvidenceText = CleanEvidenceText(match.Chunk.Content),
                PageNumber = match.Chunk.PageNumber,
                SectionTitle = match.Chunk.SectionTitle,
                SimilarityScore = Math.Round(match.Score, 4),
                SourceTitle = match.Chunk.PaperTitle,
                SourceDoi = match.Chunk.Doi,
                RetrievalMethod = "Hybrid Vector + Keyword Semantic Search",
                RelevanceExplanation = $"Retrieved from section '{match.Chunk.SectionTitle}' with {(int)(match.Score * 100)}% semantic match to claim."
            })
            .ToList();

        return topResults;
    }

    private static double ComputeCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length || vectorA.Length == 0)
            return 0.0;

        double dotProduct = 0.0;
        double normA = 0.0;
        double normB = 0.0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            normA += vectorA[i] * vectorA[i];
            normB += vectorB[i] * vectorB[i];
        }

        if (normA <= 1e-9 || normB <= 1e-9)
            return 0.0;

        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    private static HashSet<string> ExtractSignificantTokens(string text)
    {
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "the", "in", "on", "at", "to", "for", "of", "and", "or", "is", "are", "was", "were", "by", "that", "this", "it"
        };

        var tokens = text.ToLowerInvariant()
            .Split(new[] { ' ', '.', ',', ';', ':', '!', '?', '(', ')', '"', '\'', '-', '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length > 2 && !stopWords.Contains(t));

        return new HashSet<string>(tokens, StringComparer.OrdinalIgnoreCase);
    }

    private static double ComputeKeywordOverlapScore(HashSet<string> claimTokens, string chunkContent)
    {
        if (claimTokens.Count == 0 || string.IsNullOrWhiteSpace(chunkContent))
            return 0.0;

        var chunkLower = chunkContent.ToLowerInvariant();
        int matches = 0;

        foreach (var token in claimTokens)
        {
            if (chunkLower.Contains(token))
            {
                matches++;
            }
        }

        return (double)matches / claimTokens.Count;
    }

    private static string CleanEvidenceText(string chunkContent)
    {
        // Strip leading [SectionName] prefix if present for clean citation reading
        if (chunkContent.StartsWith("[") && chunkContent.Contains("] "))
        {
            int closingBracket = chunkContent.IndexOf("] ");
            if (closingBracket > 0)
            {
                return chunkContent.Substring(closingBracket + 2).Trim();
            }
        }
        return chunkContent.Trim();
    }
}
