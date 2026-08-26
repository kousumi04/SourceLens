using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SourceLensAPI.Models;
using SourceLens.ClaimExtraction.Workflow;

namespace SourceLensAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PapersController : ControllerBase
    {
        private readonly SourceLensDbContext _context;
        private readonly ExtractionOrchestrator _extractionOrchestrator;

        public PapersController(
            SourceLensDbContext context,
            ExtractionOrchestrator extractionOrchestrator)
        {
            _context = context;
            _extractionOrchestrator = extractionOrchestrator;
        }

        [HttpGet]
        public async Task<IActionResult> GetPapers()
        {
            var papers = await _context.ResearchPapers
                .Include(p => p.Claims)
                .ToListAsync();

            return Ok(papers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaper(int id)
        {
            var paper = await _context.ResearchPapers
                .Include(p => p.Claims)
                .FirstOrDefaultAsync(p => p.PaperId == id);

            if (paper == null)
                return NotFound();

            return Ok(paper);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPaper(
            IFormFile file,
            [FromForm] int userId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("PDF file is required.");

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only PDF files are supported.");

            try
            {
                // Create upload directory
                var uploadFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "uploads");

                Directory.CreateDirectory(uploadFolder);

                // Generate unique file name
                var storedFileName =
                    $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

                var filePath = Path.Combine(
                    uploadFolder,
                    storedFileName);

                // Save PDF
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create paper record
                var paper = new ResearchPaper
                {
                    UserId = userId,
                    Title = Path.GetFileNameWithoutExtension(file.FileName),
                    FileName = file.FileName,
                    UploadDate = DateTime.Now,
                    Status = "Processing"
                };

                _context.ResearchPapers.Add(paper);
                await _context.SaveChangesAsync();

                // Parse PDF + extract claims using AI
                var extractedClaims =
                    await _extractionOrchestrator.ProcessPaperAsync(filePath);

                // Save extracted claims to database
                foreach (var extractedClaim in extractedClaims)
                {
                    var claim = new Claim
                    {
                        PaperId = paper.PaperId,
                        ClaimText = extractedClaim.ClaimText,
                        PageNumber = extractedClaim.PageNumber
                    };

                    _context.Claims.Add(claim);
                }

                await _context.SaveChangesAsync();

                // Update paper status
                paper.Status = "Processed";
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Paper uploaded and processed successfully.",
                    paperId = paper.PaperId,
                    fileName = paper.FileName,
                    claimsExtracted = extractedClaims.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Paper processing failed.",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaper(ResearchPaper paper)
        {
            _context.ResearchPapers.Add(paper);
            await _context.SaveChangesAsync();

            return Ok(paper);
        }
    }
}