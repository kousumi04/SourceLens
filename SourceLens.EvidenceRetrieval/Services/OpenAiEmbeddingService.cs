using Azure;
using Azure.AI.OpenAI;
using SourceLens.EvidenceRetrieval.Interfaces;

namespace SourceLens.EvidenceRetrieval.Services;

/// <summary>
/// Generates vector embeddings for claims and paper passages using Azure.AI.OpenAI or deterministic local vectorization.
/// </summary>
public class OpenAiEmbeddingService : IEmbeddingService
{
    private readonly OpenAIClient? _openAiClient;
    private readonly string _modelOrDeploymentName;
    private const int FallbackVectorDimension = 384;

    public OpenAiEmbeddingService(string? apiKey = null, string? endpoint = null, string modelOrDeploymentName = "text-embedding-3-small")
    {
        _modelOrDeploymentName = modelOrDeploymentName;

        if (!string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(endpoint))
        {
            _openAiClient = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        }
        else if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _openAiClient = new OpenAIClient(apiKey);
        }
        else
        {
            _openAiClient = null;
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new float[FallbackVectorDimension];

        if (_openAiClient != null)
        {
            try
            {
                var options = new EmbeddingsOptions(_modelOrDeploymentName, new[] { text });
                var response = await _openAiClient.GetEmbeddingsAsync(options);
                if (response.Value.Data.Count > 0)
                {
                    return response.Value.Data[0].Embedding.ToArray();
                }
            }
            catch
            {
                // Fallback to local vector generation if remote API call fails
            }
        }

        return GenerateDeterministicVector(text);
    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts)
    {
        var textList = texts.ToList();
        if (textList.Count == 0)
            return new List<float[]>();

        if (_openAiClient != null)
        {
            try
            {
                var options = new EmbeddingsOptions(_modelOrDeploymentName, textList);
                var response = await _openAiClient.GetEmbeddingsAsync(options);
                return response.Value.Data.Select(d => d.Embedding.ToArray()).ToList();
            }
            catch
            {
                // Fallback
            }
        }

        return textList.Select(GenerateDeterministicVector).ToList();
    }

    /// <summary>
    /// Computes a normalized vector representation using character n-grams and hashing for semantic & lexical proximity.
    /// </summary>
    private static float[] GenerateDeterministicVector(string text)
    {
        var vector = new float[FallbackVectorDimension];
        var normalized = text.ToLowerInvariant().Trim();
        var words = normalized.Split(new[] { ' ', '.', ',', ';', ':', '-', '!', '?', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            // Word hash
            int hash = Math.Abs(word.GetHashCode());
            int idx = hash % FallbackVectorDimension;
            vector[idx] += 1.0f;

            // Character tri-grams for subword semantic similarity
            if (word.Length >= 3)
            {
                for (int i = 0; i <= word.Length - 3; i++)
                {
                    var trigram = word.Substring(i, 3);
                    int triHash = Math.Abs(trigram.GetHashCode());
                    int triIdx = triHash % FallbackVectorDimension;
                    vector[triIdx] += 0.5f;
                }
            }
        }

        // L2 Normalization
        float sumSquares = 0f;
        for (int i = 0; i < FallbackVectorDimension; i++)
        {
            sumSquares += vector[i] * vector[i];
        }

        float norm = (float)Math.Sqrt(sumSquares);
        if (norm > 1e-6f)
        {
            for (int i = 0; i < FallbackVectorDimension; i++)
            {
                vector[i] /= norm;
            }
        }

        return vector;
    }
}
