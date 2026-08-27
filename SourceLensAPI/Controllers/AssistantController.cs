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
    private const string AssistantSystemPrompt = """
You are the SourceLens research-paper assistant. Use the selected paper context as your grounding: paper metadata, extracted claims, linked evidence, verdicts, confidence scores, and source details. Answer through the configured Groq model.

For paper-specific questions, do not invent details that are absent from the context; say what is missing. Cite claim ids and evidence/source details when relevant.

When the user asks for a paper summary, evidence summary, claim explanation, or overall assessment, return Markdown using this exact structure:

**Paper:** *{paper title}* ({authors/year if available})

**Core Contribution**
{1-2 sentences describing the main contribution. If unavailable, say the context does not include it.}

**Key Findings**

| Claim | Summary | Evidence | Verdict |
| --- | --- | --- | --- |
| {claim number}. {claim text} | {short summary of what the claim says} | {specific supporting/refuting/neutral evidence and source details from context; cite claim/evidence ids} | {Supported/Refuted/Inconclusive/Needs Review} (confidence {score or unavailable}) |

**Implications**

- {implication grounded in the claims/evidence}
- {implication grounded in the claims/evidence}

**Overall Assessment**
{brief synthesis of which claims are supported, refuted, or unresolved. Mention evidence gaps.}

Rules:
- Preserve the section order and headings exactly.
- Do not add extra top-level sections.
- Include one table row per relevant claim.
- Use "Not available in context" instead of guessing.
- If the question is not paper-specific, answer normally and concisely.
""";

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

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("https://api.groq.com/openai/v1/chat/completions", new
            {
                model,
                temperature = 0.2,
                max_tokens = 900,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = AssistantSystemPrompt
                    },
                    new
                    {
                        role = "user",
                        content = $"Selected research paper context JSON:\n{paperContextJson}\n\nUser question:\n{request.Message}"
                    }
                }
            }, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Problem("Groq request timed out. Try again, or increase the backend HTTP client timeout.", statusCode: 504);
        }
        catch (HttpRequestException ex)
        {
            return Problem($"Could not reach Groq: {ex.Message}", statusCode: 502);
        }

        if (!response.IsSuccessStatusCode)
        {
            var groqError = await response.Content.ReadAsStringAsync(cancellationToken);
            return Problem(
                string.IsNullOrWhiteSpace(groqError)
                    ? "Groq could not answer the assistant request."
                    : $"Groq could not answer the assistant request: {groqError}",
                statusCode: (int)response.StatusCode);
        }

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
