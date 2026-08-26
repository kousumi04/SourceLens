using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SourceLens.ClaimExtraction.Interfaces;
using SourceLens.ClaimExtraction.Models;

namespace SourceLens.ClaimExtraction.Services;

public class AiClaimExtractorService : IClaimExtractor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AiClaimExtractorService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<List<ExtractedClaim>> ExtractAndSortClaimsAsync(
        string pageText,
        int pageNumber)
    {
        if (string.IsNullOrWhiteSpace(pageText))
            return new List<ExtractedClaim>();

        var apiKey =
            _configuration["GROQ_API_KEY"]
            ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                "GROQ_API_KEY is not configured.");

        var model =
            _configuration["GROQ_MODEL"]
            ?? Environment.GetEnvironmentVariable("GROQ_MODEL")
            ?? "openai/gpt-oss-20b";

        // Limit page text so one large PDF page does not consume
        // the entire Groq token-per-minute limit.
        const int maxCharacters = 6000;

        if (pageText.Length > maxCharacters)
        {
            pageText = pageText[..maxCharacters];
        }

        using var client = _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var systemPrompt = """
        Extract only important scientific claims from this research paper page.

        Categorize each claim:

        0 = Background
        1 = Methodology
        2 = Finding
        3 = Hypothesis
        4 = Conclusion

        Return ONLY valid JSON.

        Return at most 3 claims.

        Format:
        [
          {
            "claimText": "claim",
            "category": 2
          }
        ]

        If there are no important claims, return [].
        """;

        var requestBody = new
        {
            model = model,
            temperature = 0,
            max_tokens = 500,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = systemPrompt
                },
                new
                {
                    role = "user",
                    content =
                        $"Page {pageNumber}:\n{pageText}"
                }
            }
        };

        HttpResponseMessage response = null!;

        // Retry a few times when Groq temporarily returns 429.
        for (int attempt = 1; attempt <= 4; attempt++)
        {
            response = await client.PostAsJsonAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                requestBody);

            if (response.IsSuccessStatusCode)
                break;

            if ((int)response.StatusCode == 429)
            {
                Console.WriteLine(
                    $"Groq rate limit reached. Waiting before retry {attempt}/4...");

                await Task.Delay(TimeSpan.FromSeconds(6));
                continue;
            }

            var error =
                await response.Content.ReadAsStringAsync();

            throw new Exception(
                $"Groq claim extraction failed: " +
                $"{response.StatusCode}. {error}");
        }

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new Exception(
                $"Groq claim extraction failed after retries: " +
                $"{response.StatusCode}. {error}");
        }

        var responseText =
            await response.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(responseText);

        var content = json.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
            return new List<ExtractedClaim>();

        content = content.Trim();

        // Remove markdown fences if the model adds them.
        if (content.StartsWith("```"))
        {
            var firstNewLine = content.IndexOf('\n');

            if (firstNewLine >= 0)
                content = content[(firstNewLine + 1)..];

            if (content.EndsWith("```"))
                content = content[..^3];

            content = content.Trim();
        }

        Console.WriteLine();
        Console.WriteLine($"===== GROQ PAGE {pageNumber} =====");
        Console.WriteLine(content);
        Console.WriteLine("================================");
        Console.WriteLine();

        try
        {
            var claims =
                JsonSerializer.Deserialize<List<ExtractedClaim>>(
                    content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (claims == null)
                return new List<ExtractedClaim>();

            foreach (var claim in claims)
            {
                claim.PageNumber = pageNumber;
            }

            return claims;
        }
        catch (JsonException)
        {
            // If the model returned incomplete JSON, do not crash
            // the entire PDF processing operation.
            Console.WriteLine(
                $"Warning: Invalid/incomplete JSON returned for page {pageNumber}.");

            return new List<ExtractedClaim>();
        }
    }
}