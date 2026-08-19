using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SourceLensAPI.Models;

namespace SourceLensAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClaimAssessmentsController : ControllerBase
    {
        private readonly SourceLensDbContext _context;

        public ClaimAssessmentsController(SourceLensDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAssessments()
        {
            var assessments = await _context.ClaimAssessments.ToListAsync();

            return Ok(assessments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAssessment(ClaimAssessment assessment)
        {
            _context.ClaimAssessments.Add(assessment);
            await _context.SaveChangesAsync();

            return Ok(assessment);
        }
    }
}