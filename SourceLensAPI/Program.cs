using Microsoft.EntityFrameworkCore;
using SourceLensAPI.Models;

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

// Add OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();