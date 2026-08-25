// Demo data used whenever the live ASP.NET Core API is not reachable,
// so the dashboard is fully browsable/interactive on its own.

export const mockUsers = [
  { id: 1, name: "Aisha Rahman", email: "aisha.rahman@sourcelens.dev", role: "Admin" },
  { id: 2, name: "Devon Clarke", email: "devon.clarke@sourcelens.dev", role: "Researcher" },
  { id: 3, name: "Priya Nair", email: "priya.nair@sourcelens.dev", role: "Researcher" },
  { id: 4, name: "Marcus Lee", email: "marcus.lee@sourcelens.dev", role: "Reviewer" },
];

export const mockPapers = [
  {
    id: 1,
    title: "Adaptive Retrieval Improves Factual Accuracy in Long-Context QA",
    authors: "T. Osei, L. Fenwick",
    year: 2024,
    journal: "Journal of Applied NLP",
    doi: "10.1109/JANLP.2024.0113",
    uploadedBy: 2,
    uploadedDate: "2026-03-02",
  },
  {
    id: 2,
    title: "Cross-Referencing Claims Against Primary Sources at Scale",
    authors: "P. Nair, S. Okoye",
    year: 2025,
    journal: "Information Systems Review",
    doi: "10.1016/ISR.2025.0087",
    uploadedBy: 3,
    uploadedDate: "2026-04-18",
  },
  {
    id: 3,
    title: "Confidence Calibration for Automated Fact Verification",
    authors: "M. Lee, A. Rahman",
    year: 2025,
    journal: "Proc. of Trustworthy AI Workshop",
    doi: "10.5555/TAW.2025.0042",
    uploadedBy: 2,
    uploadedDate: "2026-05-27",
  },
];

export const mockSources = [
  { id: 1, title: "Benchmark Study on Retrieval-Augmented Generation", url: "https://example.org/rag-benchmark", type: "Journal Article" },
  { id: 2, title: "NLP Evaluation Dataset v3", url: "https://example.org/nlp-eval-v3", type: "Dataset" },
  { id: 3, title: "Field Report: Fact-Checking Pipelines in Production", url: "https://example.org/factcheck-report", type: "Report" },
  { id: 4, title: "Preprint: Claim Extraction from Scientific Text", url: "https://example.org/claim-extraction-preprint", type: "Preprint" },
];

export const mockClaims = [
  { id: 1, paperId: 1, text: "Adaptive retrieval improves answer accuracy by 20% over static retrieval.", extractedFrom: "Results" },
  { id: 2, paperId: 1, text: "Latency increases by less than 8% under the adaptive scheme.", extractedFrom: "Discussion" },
  { id: 3, paperId: 2, text: "Automated cross-referencing reduces manual verification time by half.", extractedFrom: "Abstract" },
  { id: 4, paperId: 3, text: "Calibrated confidence scores correlate with human-judged reliability at r = 0.86.", extractedFrom: "Results" },
];

export const mockEvidence = [
  { id: 1, claimId: 1, sourceId: 1, text: "Independent benchmark reported a 20% improvement in answer accuracy using adaptive retrieval.", supportType: "Supports" },
  { id: 2, claimId: 1, sourceId: 2, text: "Evaluation dataset confirms comparable accuracy gains across three model families.", supportType: "Supports" },
  { id: 3, claimId: 2, sourceId: 3, text: "Field report notes latency overhead closer to 15% in production settings.", supportType: "Refutes" },
  { id: 4, claimId: 3, sourceId: 3, text: "Field report corroborates reduced manual review time with automated cross-referencing.", supportType: "Supports" },
  { id: 5, claimId: 4, sourceId: 4, text: "Preprint does not report a comparable correlation figure for calibration.", supportType: "Neutral" },
];

export const mockAssessments = [
  { id: 1, claimId: 1, verdict: "Supported", confidence: 0.92, summary: "Two independent sources corroborate the 20% accuracy improvement." },
  { id: 2, claimId: 2, verdict: "Refuted", confidence: 0.71, summary: "Production field data shows notably higher latency overhead than claimed." },
  { id: 3, claimId: 3, verdict: "Supported", confidence: 0.85, summary: "Field report aligns with the claimed reduction in manual verification time." },
  { id: 4, claimId: 4, verdict: "Inconclusive", confidence: 0.48, summary: "Available sources don't directly confirm or contradict the correlation figure." },
];
