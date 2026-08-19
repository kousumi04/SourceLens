using System;
using System.Collections.Generic;

namespace SourceLensAPI.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public virtual ICollection<ResearchPaper> ResearchPapers { get; set; } = new List<ResearchPaper>();
}
