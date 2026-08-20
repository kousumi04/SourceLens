using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SourceLensAPI.Models;

namespace SourceLensAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvidenceController : ControllerBase
    {
        private readonly SourceLensDbContext _context;

        public EvidenceController(SourceLensDbContext context)
        {
            _context = context;
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
            var evidence = await _context.Evidences.ToListAsync();

            return Ok(evidence);
        }
    }
}