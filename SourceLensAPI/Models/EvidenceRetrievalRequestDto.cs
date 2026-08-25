namespace SourceLensAPI.Models;

public class EvidenceRetrievalRequestDto
{
    public int ClaimId { get; set; }
    public string ClaimText { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public string? CitedPaperTitle { get; set; }
    public string? CitedPaperDoi { get; set; }
    public int TopK { get; set; } = 3;
    public float MinSimilarityThreshold { get; set; } = 0.45f;
    public bool AutoSaveToDatabase { get; set; } = true;
}
