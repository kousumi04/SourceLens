namespace SourceLens.EvidenceRetrieval.Interfaces;

/// <summary>
/// Service interface for generating vector embeddings for text snippets (claims and paper passages).
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Generates a normalized vector embedding for a given text snippet.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <returns>A float array representing the vector embedding.</returns>
    Task<float[]> GenerateEmbeddingAsync(string text);

    /// <summary>
    /// Generates vector embeddings for a batch of text snippets.
    /// </summary>
    /// <param name="texts">Collection of text strings.</param>
    /// <returns>A list of float arrays corresponding to each input text.</returns>
    Task<List<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts);
}
