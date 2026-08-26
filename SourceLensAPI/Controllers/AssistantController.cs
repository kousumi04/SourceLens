using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SourceLensAPI.Models;

namespace SourceLensAPI.Controllers;

[ApiController]
[Route("api/assistant")]
public class AssistantController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    SourceLensDbContext dbContext) : ControllerBase
{
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] AssistantChatRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required.");

        var apiKey = configuration["GROQ_API_KEY"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            return Problem("GROQ_API_KEY is not configured on the server.", statusCode: 503);

        var model = configuration["GROQ_MODEL"] ?? Environment.GetEnvironmentVariable("GROQ_MODEL") ?? "openai/gpt-oss-120b";
        var paperContext = await ResolvePaperContextAsync(request, cancellationToken);
        var paperContextJson = JsonSerializer.Serialize(paperContext, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await client.PostAsJsonAsync("https://api.groq.com/openai/v1/chat/completions", new
        {
            model,
            temperature = 0.2,
            max_tokens = 500,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = "You are the SourceLens research-paper assistant. Use the selected paper context as your grounding: paper metadata, extracted claims, linked evidence, verdicts, confidence scores, and source details. Answer through the configured Groq model. For paper-specific questions, do not invent details that are absent from the context; say what is missing. For general questions, answer normally and connect the answer to the selected paper when useful. Keep answers concise and cite claim or evidence text when relevant."
                },
                new
                {
                    role = "user",
                    content = $"Selected research paper context JSON:\n{paperContextJson}\n\nUser question:\n{request.Message}"
                }
            }
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Problem("Groq could not answer the assistant request.", statusCode: (int)response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var answer = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

        return Ok(new { answer });
    }

    private async Task<object> ResolvePaperContextAsync(AssistantChatRequest request, CancellationToken cancellationToken)
    {
        if (request.PaperId is not null)
        {
            try
            {
                var paper = await dbContext.ResearchPapers
                    .AsNoTracking()
                    .Include(p => p.Claims)
                        .ThenInclude(c => c.ClaimAssessments)
                            .ThenInclude(a => a.Evidence)
                                .ThenInclude(e => e!.Source)
                    .FirstOrDefaultAsync(p => p.PaperId == request.PaperId.Value, cancellationToken);

                if (paper is not null)
                {
                    return new
                    {
                        paper = new
                        {
                            id = paper.PaperId,
                            title = paper.Title,
                            fileName = paper.FileName,
                            uploadedDate = paper.UploadDate,
                            status = paper.Status
                        },
                        claims = paper.Claims.Select(claim => new
                        {
                            id = claim.ClaimId,
                            text = claim.ClaimText,
                            pageNumber = claim.PageNumber,
                            assessments = claim.ClaimAssessments.Select(assessment => new
                            {
                                id = assessment.AssessmentId,
                                verdict = assessment.Verdict,
                                confidence = assessment.ConfidenceScore,
                                explanation = assessment.Explanation,
                                evidence = assessment.Evidence is null ? null : new
                                {
                                    id = assessment.Evidence.EvidenceId,
                                    text = assessment.Evidence.EvidenceText,
                                    pageNumber = assessment.Evidence.PageNumber,
                                    source = assessment.Evidence.Source is null ? null : new
                                    {
                                        id = assessment.Evidence.Source.SourceId,
                                        title = assessment.Evidence.Source.Title,
                                        authors = assessment.Evidence.Source.Authors,
                                        year = assessment.Evidence.Source.PublicationYear,
                                        doi = assessment.Evidence.Source.Doi,
                                        type = assessment.Evidence.Source.SourceType
                                    }
                                }
                            })
                        }),
                        contextSource = "database"
                    };
                }
            }
            catch
            {
                // Fall through to the request context so the assistant still works with frontend-loaded data.
            }
        }

        if (request.Context.ValueKind is not JsonValueKind.Undefined and not JsonValueKind.Null)
            return new { selectedPaperContext = request.Context, contextSource = "request" };

        return new { contextSource = "none", note = "No selected paper context was provided." };
    }
}

public sealed record AssistantChatRequest(string Message, int? PaperId, JsonElement Context);
