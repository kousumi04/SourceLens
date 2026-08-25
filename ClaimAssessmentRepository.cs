using Microsoft.EntityFrameworkCore;
using SourceLensAPI.Models;

namespace SourceLensAPI.Services
{
    public interface IClaimAssessmentRepository
    {
        Task<ClaimAssessment> SaveAsync(ClaimAssessment assessment);
        Task<ClaimAssessment?> GetByClaimIdAsync(int claimId);
    }

    /// <summary>
    /// Phase 4 — Store ClaimAssessment.
    ///
    /// This is written against EF Core's DbContext pattern used elsewhere in the
    /// project. If your DbContext is called something other than
    /// "SourceLensDbContext", or ClaimAssessment is already a mapped entity there,
    /// just point this at that DbSet instead of adding a new one.
    ///
    /// Example DbContext addition:
    ///   public DbSet&lt;ClaimAssessment&gt; ClaimAssessments =&gt; Set&lt;ClaimAssessment&gt;();
    /// </summary>
    public class ClaimAssessmentRepository : IClaimAssessmentRepository
    {
        private readonly SourceLensDbContext _db;

        public ClaimAssessmentRepository(SourceLensDbContext db)
        {
            _db = db;
        }

        public async Task<ClaimAssessment> SaveAsync(ClaimAssessment assessment)
        {
            var existing = await GetByClaimIdAsync(assessment.ClaimId);
            if (existing != null)
            {
                // Overwrite previous verification result for this claim rather than
                // accumulating duplicates — a claim has one current assessment.
                existing.Verdict = assessment.Verdict;
                existing.Confidence = assessment.Confidence;
                existing.Summary = assessment.Summary;
                existing.ClaimedValue = assessment.ClaimedValue;
                existing.CalculatedValue = assessment.CalculatedValue;
                existing.NumericUnit = assessment.NumericUnit;
                existing.DifferencePercent = assessment.DifferencePercent;
                existing.ComparisonResult = assessment.ComparisonResult;
                existing.VerificationStatus = assessment.VerificationStatus;
                existing.CreatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return existing;
            }

            _db.ClaimAssessments.Add(assessment);
            await _db.SaveChangesAsync();
            return assessment;
        }

        public async Task<ClaimAssessment?> GetByClaimIdAsync(int claimId)
        {
            return await _db.ClaimAssessments
                .Where(a => a.ClaimId == claimId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
