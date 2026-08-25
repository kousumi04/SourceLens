using SourceLens.EvidenceRetrieval.Interfaces;
using SourceLens.EvidenceRetrieval.Models;

namespace SourceLens.EvidenceRetrieval.Services;

/// <summary>
/// Chunks academic paper text into overlapping passages suitable for embedding generation and vector search.
/// </summary>
public class TextChunkerService : ITextChunker
{
    public List<PaperChunk> ChunkPaper(CitedPaper paper, int maxWordsPerChunk = 120, int overlapWords = 25)
    {
        var chunks = new List<PaperChunk>();
        var chunkIndex = 0;

        // If structured sections are present, chunk per section to preserve section headers
        if (paper.Sections != null && paper.Sections.Count > 0)
        {
            var pageEstimate = 1;
            foreach (var (sectionTitle, sectionContent) in paper.Sections)
            {
                if (string.IsNullOrWhiteSpace(sectionContent))
                    continue;

                var sectionChunks = ChunkText(sectionContent, maxWordsPerChunk, overlapWords);
                foreach (var chunkText in sectionChunks)
                {
                    chunks.Add(new PaperChunk
                    {
                        ChunkId = $"{paper.PaperId}_chunk_{chunkIndex}",
                        PaperId = paper.PaperId,
                        PaperTitle = paper.Title,
                        Doi = paper.Doi,
                        SectionTitle = sectionTitle,
                        PageNumber = pageEstimate,
                        ChunkIndex = chunkIndex++,
                        Content = $"[{sectionTitle}] {chunkText}"
                    });
                }
                pageEstimate++;
            }
        }
        else if (!string.IsNullOrWhiteSpace(paper.FullText))
        {
            var textChunks = ChunkText(paper.FullText, maxWordsPerChunk, overlapWords);
            for (int i = 0; i < textChunks.Count; i++)
            {
                chunks.Add(new PaperChunk
                {
                    ChunkId = $"{paper.PaperId}_chunk_{chunkIndex}",
                    PaperId = paper.PaperId,
                    PaperTitle = paper.Title,
                    Doi = paper.Doi,
                    SectionTitle = "Main Body",
                    PageNumber = (i / 3) + 1,
                    ChunkIndex = chunkIndex++,
                    Content = textChunks[i]
                });
            }
        }

        return chunks;
    }

    private static List<string> ChunkText(string text, int maxWordsPerChunk, int overlapWords)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(text))
            return result;

        var words = text.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= maxWordsPerChunk)
        {
            result.Add(string.Join(" ", words));
            return result;
        }

        int step = Math.Max(1, maxWordsPerChunk - overlapWords);
        for (int i = 0; i < words.Length; i += step)
        {
            int count = Math.Min(maxWordsPerChunk, words.Length - i);
            var chunk = string.Join(" ", words.Skip(i).Take(count));
            result.Add(chunk);

            if (i + count >= words.Length)
                break;
        }

        return result;
    }
}
