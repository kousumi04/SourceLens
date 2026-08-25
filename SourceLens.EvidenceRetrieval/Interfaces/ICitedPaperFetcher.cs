using SourceLens.EvidenceRetrieval.Models;

namespace SourceLens.EvidenceRetrieval.Interfaces;

/// <summary>
/// Service interface for querying and fetching cited papers from academic APIs (e.g. Semantic Scholar, OpenAlex, arXiv).
/// </summary>
public interface ICitedPaperFetcher
{
    /// <summary>
    /// Fetches a cited paper by its DOI or ArXiv identifier.
    /// </summary>
    /// <param name="doiOrArxivId">The DOI string or arXiv identifier.</param>
    /// <returns>The fetched paper data or null if not found.</returns>
    Task<CitedPaper?> FetchPaperByDoiAsync(string doiOrArxivId);

    /// <summary>
    /// Searches for cited papers matching a title, author, or keyword query.
    /// </summary>
    /// <param name="query">Title or citation query string.</param>
    /// <param name="maxResults">Maximum number of candidate papers to return.</param>
    /// <returns>List of candidate cited papers.</returns>
    Task<List<CitedPaper>> SearchAndFetchPapersAsync(string query, int maxResults = 3);
}
