using Microsoft.EntityFrameworkCore;

namespace SourceLensAPI.Models
{
    /// <summary>
    /// STUB — do not add this file if SourceLensDbContext already exists in the
    /// project. Instead, add the single DbSet line below to your real context so
    /// EF Core knows about ClaimAssessment (or map the new numeric fields onto the
    /// ClaimAssessment entity you already have).
    /// </summary>
    public class SourceLensDbContext : DbContext
    {
        public SourceLensDbContext(DbContextOptions<SourceLensDbContext> options) : base(options) { }

        public DbSet<ClaimAssessment> ClaimAssessments => Set<ClaimAssessment>();

        // Existing sets expected elsewhere in the project (Users, Papers, Claims,
        // Sources, Evidence) already live in the real DbContext — not duplicated here.
    }
}
