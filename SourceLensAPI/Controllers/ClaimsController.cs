using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SourceLensAPI.Models;

namespace SourceLensAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly SourceLensDbContext _context;

        public ClaimsController(SourceLensDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClaim(Claim claim)
        {
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return Ok(claim);
        }

        [HttpGet("/api/Papers/{paperId}/claims")]
        public async Task<IActionResult> GetClaimsByPaper(int paperId)
        {
            var claims = await _context.Claims
                .Where(c => c.PaperId == paperId)
                .ToListAsync();

            return Ok(claims);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClaim(int id)
        {
            var claim = await _context.Claims.FindAsync(id);

            if (claim == null)
                return NotFound();

            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}