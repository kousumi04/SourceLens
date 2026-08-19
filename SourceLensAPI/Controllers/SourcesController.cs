using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SourceLensAPI.Models;

namespace SourceLensAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SourcesController : ControllerBase
    {
        private readonly SourceLensDbContext _context;

        public SourcesController(SourceLensDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSource(Source source)
        {
            _context.Sources.Add(source);
            await _context.SaveChangesAsync();

            return Ok(source);
        }

        [HttpGet]
        public async Task<IActionResult> GetSources()
        {
            var sources = await _context.Sources.ToListAsync();

            return Ok(sources);
        }
    }
}