using SourceLensAPI.Models;

namespace SourceLensAPI.Interfaces
{
    /// <summary>
    /// Phase 6 — Evidence/RAG Integration.
    /// This is the seam between the numeric-verification module and the teammates'
    /// Evidence/RAG module. Numeric verification does NOT need to know how evidence
    /// retrieval works internally (embeddings, vector store, whatever) — it only
    /// needs this contract implemented against the existing Evidence table
    /// (claimId, sourceId, text, supportType).
    ///
    /// Whoever owns the RAG module implements this interface and registers it in
    /// DI (see Program.cs), e.g.:
    ///   builder.Services.AddScoped&lt;IEvidenceRagService, EvidenceRagService&gt;();
    /// </summary>
    public interface IEvidenceRagService
    {
        /// <summary>
        /// Retrieves the evidence passages relevant to a claim, so the numeric result
        /// can be cross-checked against what independent sources actually report —
        /// not just against the claim's own internal arithmetic.
        /// </summary>
        Task<List<EvidenceSnippet>> GetSupportingEvidenceAsync(int claimId, string claimText);
    }

    /// <summary>
    /// Lightweight fallback used when the real RAG service isn't wired up yet
    /// (e.g. teammate's module isn't merged). Lets the rest of the pipeline run
    /// end-to-end during development. Register the real implementation instead
    /// once it's ready — nothing else in this module needs to change.
    /// </summary>
    public class NullEvidenceRagService : IEvidenceRagService
    {
        public Task<List<EvidenceSnippet>> GetSupportingEvidenceAsync(int claimId, string claimText)
            => Task.FromResult(new List<EvidenceSnippet>());
    }
}
