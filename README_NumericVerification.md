# Numeric Verification Module

Implements the 6-phase pipeline for SourceLens's ASP.NET Core backend:

```
Text/Claim -> Extract Numbers -> Calculate -> Compare -> Assess & Store -> Verify with Evidence/RAG
```

## Files -> Phases

| Phase | File |
|---|---|
| 1. Manual/Structured Numbers | `Models/NumericValue.cs` — the shared shape; pass these in directly on `NumericVerificationRequest.Numbers` |
| 2. Numeric Calculation Engine | `Services/CalculationEngine.cs` — Sum, Average, Difference, PercentageChange, Ratio, Identity |
| 3. Compare Claimed vs. Calculated | `Services/ComparisonService.cs` — Match / Approximate / Mismatch, tolerance-based |
| 4. Store ClaimAssessment | `Services/ClaimAssessmentRepository.cs`, `Models/CalculationResult.cs` (`ClaimAssessment` class) |
| 5. Automatic Number Extraction | `Services/NumberExtractionService.cs` — regex-based, no NLP dependency required |
| 6. Evidence/RAG Integration | `Interfaces/IEvidenceRagService.cs` — the contract your teammates' module implements |

`Services/NumericVerificationService.cs` is the orchestrator that runs all six in order.
`Controllers/NumericVerificationController.cs` exposes it as `POST /api/NumericVerification/verify`.

## Wiring it in

1. Copy `Models/`, `Services/`, `Controllers/`, `Interfaces/` into your existing
   ASP.NET Core project (merge folders, don't overwrite).
2. **If `SourceLensDbContext` already exists**, delete `Models/SourceLensDbContext.cs`
   (it's a stub) and instead add this line to your real context:
   ```csharp
   public DbSet<ClaimAssessment> ClaimAssessments => Set<ClaimAssessment>();
   ```
   If `ClaimAssessment` is already an EF entity with `verdict`/`confidence`/`summary`,
   add the numeric fields from `Models/CalculationResult.cs` (`ClaimedValue`,
   `CalculatedValue`, `NumericUnit`, `DifferencePercent`, `ComparisonResult`,
   `VerificationStatus`) to that entity and delete the duplicate class here.
3. Run `dotnet ef migrations add AddNumericVerificationFields` and
   `dotnet ef database update`.
4. Paste the contents of `ProgramRegistration.cs.txt` into `Program.cs`.
5. Whoever owns the Evidence/RAG module implements `IEvidenceRagService`
   (`GetSupportingEvidenceAsync(claimId, claimText)` -> `List<EvidenceSnippet>`,
   built from the existing `Evidence` table). Until then, `NullEvidenceRagService`
   is registered so the pipeline runs end-to-end returning no evidence.

## Example request

```
POST /api/NumericVerification/verify
{
  "claimId": 1,
  "claimText": "Adaptive retrieval improves answer accuracy by 20% over static retrieval.",
  "operation": "Identity",
  "claimedValue": 20,
  "unit": "Percentage",
  "useEvidence": true
}
```

`operation: "Identity"` means "the claim states one number outright — check that
number against evidence/tolerance, no arithmetic needed." Use `PercentageChange`,
`Ratio`, `Sum`, etc. when the claimed number is derived from two or more values
(e.g. "before: 65, after: 78" -> claim "20% improvement" needs `PercentageChange`
over `[65, 78]`).

## Frontend hookup

In `sourcelens-frontend`, add a call in a new page or inside `Claims.jsx`:

```js
import { api } from "../api/client";

async function verifyClaim(claimId, claimText, claimedValue, unit) {
  const res = await api.post("/NumericVerification/verify", {
    claimId, claimText, claimedValue, unit, operation: "Identity", useEvidence: true,
  });
  return res.data; // { calculation, comparison, assessment, evidence }
}
```

`assessment` in the response matches the shape already used by
`mockAssessments` (`verdict`, `confidence`, `summary`) plus the new numeric
fields, so it can be rendered with the existing `VerdictBadge` /
`ConfidenceDial` components without changes.
