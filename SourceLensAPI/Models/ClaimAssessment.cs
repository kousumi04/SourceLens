using System;

namespace SourceLensAPI.Models;

public partial class ClaimAssessment
{
    public int AssessmentId { get; set; }

    public int ClaimId { get; set; }

    public int EvidenceId { get; set; }

    public string Verdict { get; set; } = null!;

    public double ConfidenceScore { get; set; }

    public string Explanation { get; set; } = null!;

    public virtual Claim? Claim { get; set; }

    public virtual Evidence? Evidence { get; set; }
}