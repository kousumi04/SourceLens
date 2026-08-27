using Microsoft.EntityFrameworkCore;
using SourceLensAPI.Models;
using Scalar.AspNetCore;

DotEnv.Load();

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

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SourceLensDbContext>();
    dbContext.Database.EnsureCreated();
}

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

file static class DotEnv
{
    public static void Load()
    {
        var dotenvPath = FindDotEnvFile();
        if (dotenvPath is null)
            return;

        foreach (var line in File.ReadLines(dotenvPath))
        {
            var parsed = ParseLine(line);
            if (parsed is null)
                continue;

            var (key, value) = parsed.Value;
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static string? FindDotEnvFile()
    {
        var directories = new[]
        {
            new DirectoryInfo(Directory.GetCurrentDirectory()),
            new DirectoryInfo(AppContext.BaseDirectory)
        };

        foreach (var startDirectory in directories)
        {
            var directory = startDirectory;

            while (directory is not null)
            {
                var candidates = new[]
                {
                    Path.Combine(directory.FullName, ".env"),
                    Path.Combine(directory.FullName, "SourceLensAPI", ".env")
                };

                var path = candidates.FirstOrDefault(File.Exists);
                if (path is not null)
                    return path;

                directory = directory.Parent;
            }
        }

        return null;
    }

    private static (string Key, string Value)? ParseLine(string line)
    {
        var trimmed = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
            return null;

        const string exportPrefix = "export ";
        if (trimmed.StartsWith(exportPrefix, StringComparison.OrdinalIgnoreCase))
            trimmed = trimmed[exportPrefix.Length..].TrimStart();

        var separatorIndex = trimmed.IndexOf('=');
        if (separatorIndex <= 0)
            return null;

        var key = trimmed[..separatorIndex].Trim().TrimStart('\uFEFF');
        var value = trimmed[(separatorIndex + 1)..].Trim();

        if (value.Length >= 2)
        {
            var quote = value[0];
            if ((quote == '"' || quote == '\'') && value[^1] == quote)
                value = value[1..^1];
            else
                value = StripInlineComment(value);
        }
        else
        {
            value = StripInlineComment(value);
        }

        return string.IsNullOrWhiteSpace(key) ? null : (key, value);
    }

    private static string StripInlineComment(string value)
    {
        var commentIndex = value.IndexOf(" #", StringComparison.Ordinal);
        return commentIndex >= 0 ? value[..commentIndex].TrimEnd() : value;
    }
}
