using System;
using System.Collections.Generic;

namespace SourceLensAPI.Models;

public partial class Source
{
    public int SourceId { get; set; }

    public string Title { get; set; } = null!;

    public string Authors { get; set; } = null!;

    public int? PublicationYear { get; set; }

    public string? Doi { get; set; }

    public string SourceType { get; set; } = null!;

    public virtual ICollection<Citation> Citations { get; set; } = new List<Citation>();

    public virtual ICollection<Evidence> Evidences { get; set; } = new List<Evidence>();
}
