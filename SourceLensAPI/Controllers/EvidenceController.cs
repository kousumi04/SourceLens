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

        [HttpPost]
        public async Task<IActionResult> CreateEvidence(Evidence evidence)
        {
            _context.Evidences.Add(evidence);
            await _context.SaveChangesAsync();

            return Ok(evidence);
        }

        [HttpGet]
        public async Task<IActionResult> GetEvidence()
        {
            var evidence = await _context.Evidences
                .Include(e => e.Source)
                .ToListAsync();

            return Ok(evidence);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvidenceById(int id)
        {
            var evidence = await _context.Evidences
                .Include(e => e.Source)
                .FirstOrDefaultAsync(e => e.EvidenceId == id);

            if (evidence == null)
                return NotFound($"Evidence with ID {id} not found.");

            return Ok(evidence);
        }

        /// <summary>
        /// Executes the RAG pipeline: fetches cited paper, chunks content, runs vector similarity search,
        /// and optionally saves top retrieved evidence directly into the database.
        /// </summary>
        [HttpPost("retrieve")]
        public async Task<IActionResult> RetrieveEvidence([FromBody] EvidenceRetrievalRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.ClaimText))
            {
                // If claim text not provided in body, try to load from database by ClaimId
                if (request.ClaimId > 0)
                {
                    var claim = await _context.Claims.FindAsync(request.ClaimId);
                    if (claim != null)
                    {
                        request.ClaimText = claim.ClaimText;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(request.ClaimText))
            {
                return BadRequest("ClaimText or a valid ClaimId must be provided.");
            }

            // If sourceId is provided but DOI/Title not specified, fetch source info from DB
            if (request.SourceId > 0 && string.IsNullOrEmpty(request.CitedPaperTitle) && string.IsNullOrEmpty(request.CitedPaperDoi))
            {
                var source = await _context.Sources.FindAsync(request.SourceId);
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
                MinSimilarityThreshold = request.MinSimilarityThreshold
            };

            var retrievedResults = await _retrievalOrchestrator.ProcessAndRetrieveEvidenceAsync(query);

            // Optionally persist retrieved evidence into the database
            if (request.AutoSaveToDatabase && retrievedResults.Count > 0)
            {
                // Ensure a valid SourceId exists, create if needed
                int sourceId = request.SourceId;
                if (sourceId <= 0)
                {
                    var firstMatch = retrievedResults.First();
                    var newSource = new Source
                    {
                        Title = string.IsNullOrWhiteSpace(firstMatch.SourceTitle) ? (request.CitedPaperTitle ?? "Cited Academic Paper") : firstMatch.SourceTitle,
                        Authors = string.IsNullOrWhiteSpace(firstMatch.SourceAuthors) ? "Academic Authors" : firstMatch.SourceAuthors,
                        PublicationYear = firstMatch.PublicationYear ?? DateTime.UtcNow.Year,
                        Doi = firstMatch.SourceDoi ?? request.CitedPaperDoi,
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
    }
}