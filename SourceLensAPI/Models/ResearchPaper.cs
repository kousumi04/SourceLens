using System;
using System.Collections.Generic;

namespace SourceLensAPI.Models;

public partial class ResearchPaper
{
    public int PaperId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public DateTime UploadDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

    public virtual User? User { get; set; }
}