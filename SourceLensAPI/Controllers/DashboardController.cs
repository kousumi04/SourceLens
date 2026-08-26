using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace SourceLensAPI.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : ControllerBase
{
    [HttpPost("summary")]
    public async Task<IActionResult> Summarize([FromBody] DashboardSummaryRequest request, CancellationToken cancellationToken)
    {
        var apiKey = configuration["GROQ_API_KEY"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            return Problem("GROQ_API_KEY is not configured on the server.", statusCode: 503);

        var model = configuration["GROQ_MODEL"] ?? Environment.GetEnvironmentVariable("GROQ_MODEL") ?? "openai/gpt-oss-120b";
        var prompt = "Summarize this SourceLens dashboard in exactly 5 or 6 short lines. Use plain, simple English. Include the key counts, confidence, verdict distribution, and the most important recent claim findings. Do not use bullets, numbering, headings, or markdown.\n\nDashboard snapshot:\n" +
                     JsonSerializer.Serialize(request);

        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        using var response = await client.PostAsJsonAsync("https://api.groq.com/openai/v1/chat/completions", new
        {
            model,
            temperature = 0.2,
            max_tokens = 300,
            messages = new[]
            {
                new { role = "system", content = "You summarize dashboard data accurately and concisely." },
                new { role = "user", content = prompt }
            }
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Problem("Groq could not create the summary.", statusCode: (int)response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var summary = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        return Ok(new { summary });
    }
}

public sealed record DashboardSummaryRequest(
    int Papers,
    int Claims,
    int EvidenceLinked,
    int AssessedClaims,
    double AverageConfidence,
    object[] VerdictDistribution,
    object[] RecentClaims);
