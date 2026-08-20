using System;
using System.Collections.Generic;

namespace SourceLensAPI.Models;

public partial class Claim
{
    public int ClaimId { get; set; }

    public int PaperId { get; set; }

    public string ClaimText { get; set; } = null!;

    public int? PageNumber { get; set; }

    public virtual ICollection<Citation> Citations { get; set; } = new List<Citation>();

    public virtual ICollection<ClaimAssessment> ClaimAssessments { get; set; } = new List<ClaimAssessment>();

    public virtual ResearchPaper? Paper { get; set; }
}
