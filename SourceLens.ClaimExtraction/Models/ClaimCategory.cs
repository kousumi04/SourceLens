namespace SourceLens.ClaimExtraction.Models;

/// <summary>
/// Represents the label/type of the extracted claim.
/// </summary>
public enum ClaimCategory
{
    Background,     // Existing knowledge or context
    Methodology,    // How the research was conducted
    Finding,        // A factual result from the experiment
    Hypothesis,     // A proposed explanation made based on limited evidence
    Conclusion      // The final deduced statement by the authors
}
