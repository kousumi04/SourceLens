namespace SourceLens.ClaimExtraction.Interfaces;

public interface IPaperParser
{
    /// <summary>
    /// Parses a research paper (PDF) and returns the text grouped by page number.
    /// </summary>
    /// <param name="filePath">The path to the PDF document.</param>
    /// <returns>A dictionary mapping the page number to the extracted text.</returns>
    Task<Dictionary<int, string>> ExtractTextByPageAsync(string filePath);
}
