using System;
using System.Collections.Generic;

namespace SourceLensAPI.Models;

public partial class Citation
{
    public int CitationId { get; set; }

    public int ClaimId { get; set; }

    public int SourceId { get; set; }

    public string CitationText { get; set; } = null!;

    public virtual Claim Claim { get; set; } = null!;

    public virtual Source Source { get; set; } = null!;
}
