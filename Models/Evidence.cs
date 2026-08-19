using System;
using System.Collections.Generic;

namespace SourceLensAPI.Models;

public partial class Evidence
{
    public int EvidenceId { get; set; }

    public int SourceId { get; set; }

    public string EvidenceText { get; set; } = null!;

    public int? PageNumber { get; set; }

    public virtual ICollection<ClaimAssessment> ClaimAssessments { get; set; } = new List<ClaimAssessment>();

    public virtual Source? Source { get; set; }
}