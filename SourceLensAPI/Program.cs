using Microsoft.EntityFrameworkCore;
using SourceLensAPI.Models;
using Scalar.AspNetCore;

// Load the developer .env file when running from the repository (never commit this file).
var dotenvPaths = new[]
{
    Path.Combine(Directory.GetCurrentDirectory(), ".env"),
    Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "", ".env"),
    Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "")?.FullName ?? "", ".env")
};
var dotenvPath = dotenvPaths.FirstOrDefault(File.Exists);
if (dotenvPath is not null)
{
    foreach (var line in File.ReadLines(dotenvPath))
    {
        var parts = line.Split('=', 2);
        if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]) && string.IsNullOrEmpty(Environment.GetEnvironmentVariable(parts[0])))
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim().Trim('"'));
    }
}

var builder = WebApplication.CreateBuilder(args);

// Connect Entity Framework Core to SQL Server
builder.Services.AddDbContext<SourceLensDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SourceLensDb")));

// Register Claim Extraction Services
builder.Services.AddScoped<SourceLens.ClaimExtraction.Interfaces.IPaperParser, SourceLens.ClaimExtraction.Services.PdfParserService>();
builder.Services.AddScoped<SourceLens.ClaimExtraction.Interfaces.IClaimExtractor, SourceLens.ClaimExtraction.Services.AiClaimExtractorService>();
builder.Services.AddScoped<SourceLens.ClaimExtraction.Workflow.ExtractionOrchestrator>();

// Register Evidence Retrieval (RAG) Services
builder.Services.AddHttpClient<SourceLens.EvidenceRetrieval.Interfaces.ICitedPaperFetcher, SourceLens.EvidenceRetrieval.Services.AcademicPaperFetcherService>();
builder.Services.AddSingleton<SourceLens.EvidenceRetrieval.Interfaces.ITextChunker, SourceLens.EvidenceRetrieval.Services.TextChunkerService>();
builder.Services.AddSingleton<SourceLens.EvidenceRetrieval.Interfaces.IEmbeddingService, SourceLens.EvidenceRetrieval.Services.OpenAiEmbeddingService>();
builder.Services.AddScoped<SourceLens.EvidenceRetrieval.Interfaces.IEvidenceRetriever, SourceLens.EvidenceRetrieval.Services.RagEvidenceRetrieverService>();
builder.Services.AddScoped<SourceLens.EvidenceRetrieval.Workflow.EvidenceRetrievalOrchestrator>();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Add OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
