using SourceLens.ClaimExtraction.Interfaces;
using UglyToad.PdfPig;

namespace SourceLens.ClaimExtraction.Services;

public class PdfParserService : IPaperParser
{
    public Task<Dictionary<int, string>> ExtractTextByPageAsync(string filePath)
    {
        var pageTexts = new Dictionary<int, string>();

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file at {filePath} could not be found.");
        }

        // Using PdfPig to read the document
        using (var document = PdfDocument.Open(filePath))
        {
            foreach (var page in document.GetPages())
            {
                // Extract text from each page
                var text = page.Text;
                pageTexts[page.Number] = text;
            }
        }

        return Task.FromResult(pageTexts);
    }
}
