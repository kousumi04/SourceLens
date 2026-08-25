using Microsoft.AspNetCore.Mvc;
using SourceLensAPI.Models;
using SourceLensAPI.Services;

namespace SourceLensAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NumericVerificationController : ControllerBase
    {
        private readonly INumericVerificationService _verificationService;

        public NumericVerificationController(INumericVerificationService verificationService)
        {
            _verificationService = verificationService;
        }

        /// <summary>
        /// POST /api/NumericVerification/verify
        /// Runs the full pipeline. Send either "numbers" (manual/structured, Phase 1)
        /// or "claimText" (auto-extracted, Phase 5) — not both required.
        ///
        /// Example body:
        /// {
        ///   "claimId": 1,
        ///   "claimText": "Adaptive retrieval improves answer accuracy by 20% over static retrieval.",
        ///   "operation": "Identity",
        ///   "claimedValue": 20,
        ///   "unit": "Percentage",
        ///   "useEvidence": true
        /// }
        /// </summary>
        [HttpPost("verify")]
        public async Task<ActionResult<NumericVerificationOutcome>> Verify([FromBody] NumericVerificationRequest request)
        {
            if (request.ClaimId <= 0)
                return BadRequest("claimId is required.");

            if ((request.Numbers is null || request.Numbers.Count == 0) && string.IsNullOrWhiteSpace(request.ClaimText))
                return BadRequest("Provide either 'numbers' (structured input) or 'claimText' (for auto-extraction).");

            var outcome = await _verificationService.VerifyAsync(request);
            return Ok(outcome);
        }

        /// <summary>
        /// POST /api/NumericVerification/extract
        /// Phase 5 exposed on its own — lets the frontend preview what numbers would
        /// be pulled out of a piece of text before running the full pipeline.
        /// </summary>
        [HttpPost("extract")]
        public ActionResult<List<NumericValue>> ExtractOnly(
            [FromServices] Services.INumberExtractionService extractor,
            [FromBody] ExtractRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("text is required.");

            return Ok(extractor.Extract(request.Text));
        }

        public class ExtractRequest
        {
            public string Text { get; set; } = string.Empty;
        }
    }
}
