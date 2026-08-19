using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SourceLensAPI.Models;

namespace SourceLensAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PapersController : ControllerBase
    {
        private readonly SourceLensDbContext _context;

        public PapersController(SourceLensDbContext context)
        {
            _context = context;
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

        [HttpPost]
        public async Task<IActionResult> CreatePaper(ResearchPaper paper)
        {
            _context.ResearchPapers.Add(paper);
            await _context.SaveChangesAsync();

            return Ok(paper);
        }
    }
}