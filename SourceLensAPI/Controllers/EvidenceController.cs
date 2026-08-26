using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SourceLens.EvidenceRetrieval.Models;
using SourceLens.EvidenceRetrieval.Workflow;
using SourceLensAPI.Models;

namespace SourceLensAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvidenceController : ControllerBase
    {
        private readonly SourceLensDbContext _context;
        private readonly EvidenceRetrievalOrchestrator _retrievalOrchestrator;

        public EvidenceController(
            SourceLensDbContext context,
            EvidenceRetrievalOrchestrator retrievalOrchestrator)
        {
            _context = context;
            _retrievalOrchestrator = retrievalOrchestrator;
        }

        // GET: /api/Evidence
        [HttpGet]
        public async Task<IActionResult> GetEvidence()
        {
            try
            {
                var evidence = await _context.Evidences
                    .Select(e => new
                    {
                        evidenceId = e.EvidenceId,
                        sourceId = e.SourceId,
                        evidenceText = e.EvidenceText,
                        pageNumber = e.PageNumber,

                        source = e.Source == null
                            ? null
                            : new
                            {
                                sourceId = e.Source.SourceId,
                                title = e.Source.Title,
                                authors = e.Source.Authors,
                                publicationYear = e.Source.PublicationYear,
                                doi = e.Source.Doi,
                                sourceType = e.Source.SourceType
                            }
                    })
                    .ToListAsync();

                return Ok(evidence);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to load evidence.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // GET: /api/Evidence/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvidenceById(int id)
        {
            try
            {
                var evidence = await _context.Evidences
                    .Where(e => e.EvidenceId == id)
                    .Select(e => new
                    {
                        evidenceId = e.EvidenceId,
                        sourceId = e.SourceId,
                        evidenceText = e.EvidenceText,
                        pageNumber = e.PageNumber,

                        source = e.Source == null
                            ? null
                            : new
                            {
                                sourceId = e.Source.SourceId,
                                title = e.Source.Title,
                                authors = e.Source.Authors,
                                publicationYear = e.Source.PublicationYear,
                                doi = e.Source.Doi,
                                sourceType = e.Source.SourceType
                            }
                    })
                    .FirstOrDefaultAsync();

                if (evidence == null)
                    return NotFound(new
                    {
                        message = $"Evidence with ID {id} not found."
                    });

                return Ok(evidence);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to load evidence.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // POST: /api/Evidence
        [HttpPost]
        public async Task<IActionResult> CreateEvidence(
            [FromBody] Evidence evidence)
        {
            try
            {
                _context.Evidences.Add(evidence);
                await _context.SaveChangesAsync();

                return Ok(evidence);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to create evidence.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // POST: /api/Evidence/retrieve
        [HttpPost("retrieve")]
        public async Task<IActionResult> RetrieveEvidence(
            [FromBody] EvidenceRetrievalRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ClaimText))
                {
                    if (request.ClaimId > 0)
                    {
                        var claim = await _context.Claims
                            .FindAsync(request.ClaimId);

                        if (claim != null)
                        {
                            request.ClaimText = claim.ClaimText;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(request.ClaimText))
                {
                    return BadRequest(new
                    {
                        message =
                            "ClaimText or a valid ClaimId must be provided."
                    });
                }

                if (request.SourceId > 0 &&
                    string.IsNullOrEmpty(request.CitedPaperTitle) &&
                    string.IsNullOrEmpty(request.CitedPaperDoi))
                {
                    var source = await _context.Sources
                        .FindAsync(request.SourceId);

                    if (source != null)
                    {
                        request.CitedPaperTitle = source.Title;
                        request.CitedPaperDoi = source.Doi;
                    }
                }

                var query = new EvidenceSearchQuery
                {
                    ClaimId = request.ClaimId,
                    ClaimText = request.ClaimText,
                    SourceId = request.SourceId,
                    CitedPaperTitle = request.CitedPaperTitle,
                    CitedPaperDoi = request.CitedPaperDoi,
                    TopK = request.TopK,
                    MinSimilarityThreshold =
                        request.MinSimilarityThreshold
                };

                var retrievedResults =
                    await _retrievalOrchestrator
                        .ProcessAndRetrieveEvidenceAsync(query);

                if (request.AutoSaveToDatabase &&
                    retrievedResults.Count > 0)
                {
                    int sourceId = request.SourceId;

                    if (sourceId <= 0)
                    {
                        var firstMatch = retrievedResults.First();

                        var newSource = new Source
                        {
                            Title =
                                string.IsNullOrWhiteSpace(
                                    firstMatch.SourceTitle)
                                    ? (
                                        request.CitedPaperTitle
                                        ?? "Cited Academic Paper"
                                      )
                                    : firstMatch.SourceTitle,

                            Authors =
                                string.IsNullOrWhiteSpace(
                                    firstMatch.SourceAuthors)
                                    ? "Academic Authors"
                                    : firstMatch.SourceAuthors,

                            PublicationYear =
                                firstMatch.PublicationYear
                                ?? DateTime.UtcNow.Year,

                            Doi =
                                firstMatch.SourceDoi
                                ?? request.CitedPaperDoi,

                            SourceType = "Journal"
                        };

                        _context.Sources.Add(newSource);

                        await _context.SaveChangesAsync();

                        sourceId = newSource.SourceId;
                    }

                    foreach (var item in retrievedResults)
                    {
                        item.SourceId = sourceId;

                        var evidenceRecord = new Evidence
                        {
                            SourceId = sourceId,
                            EvidenceText = item.EvidenceText,
                            PageNumber = item.PageNumber
                        };

                        _context.Evidences.Add(evidenceRecord);
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    claimText = request.ClaimText,
                    claimId = request.ClaimId,
                    evidenceCount = retrievedResults.Count,
                    results = retrievedResults
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Evidence retrieval failed.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
    }
}