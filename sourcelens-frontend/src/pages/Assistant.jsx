import { useMemo, useRef, useState } from "react";
import Topbar from "../components/Topbar";
import DemoBanner from "../components/DemoBanner";
import VerdictBadge from "../components/VerdictBadge";
import ConfidenceDial from "../components/ConfidenceDial";
import { api } from "../api/client";
import { useResource } from "../hooks/useResource";
import {
  mockAssessments,
  mockClaims,
  mockEvidence,
  mockPapers,
  mockSources,
} from "../api/mockData";

const STARTER_MESSAGES = [
  {
    role: "assistant",
    text:
      "Choose an uploaded paper, then ask questions about it. I will answer through your configured Groq model using the paper's SourceLens context.",
  },
];

const QUICK_PROMPTS = [
  "Summarize this paper",
  "Which claims need review?",
  "Summarize the evidence",
  "What does the paper conclude?",
];

function getId(item, names) {
  for (const name of names) {
    if (item?.[name] !== undefined && item?.[name] !== null) return item[name];
  }
  return "";
}

function getText(item, names) {
  for (const name of names) {
    if (typeof item?.[name] === "string" && item[name].trim()) return item[name];
  }
  return "";
}

function normalizeWorkspace({ papers, claims, evidence, assessments, sources }) {
  const normalizedClaims = claims.map((claim) => ({
    id: getId(claim, ["id", "claimId", "ClaimId"]),
    paperId: getId(claim, ["paperId", "PaperId"]),
    text: getText(claim, ["text", "claimText", "ClaimText"]),
    section: getText(claim, ["extractedFrom", "section", "Section"]),
  }));

  const normalizedPapers = papers.map((paper) => ({
    id: getId(paper, ["id", "paperId", "PaperId"]),
    title: getText(paper, ["title", "Title"]),
    authors: getText(paper, ["authors", "Authors"]),
    year: getId(paper, ["year", "publicationYear", "PublicationYear"]),
    fileName: getText(paper, ["fileName", "FileName"]),
    status: getText(paper, ["status", "Status"]),
    uploadedDate: getText(paper, ["uploadedDate", "uploadDate", "UploadDate"]),
  }));

  const normalizedSources = sources.map((source) => ({
    id: getId(source, ["id", "sourceId", "SourceId"]),
    title: getText(source, ["title", "Title"]),
    type: getText(source, ["type", "sourceType", "SourceType"]),
  }));

  const normalizedEvidence = evidence.map((item) => ({
    id: getId(item, ["id", "evidenceId", "EvidenceId"]),
    claimId: getId(item, ["claimId", "ClaimId"]),
    sourceId: getId(item, ["sourceId", "SourceId"]),
    text: getText(item, ["text", "evidenceText", "EvidenceText"]),
    supportType: getText(item, ["supportType", "SupportType"]) || "Neutral",
  }));

  const normalizedAssessments = assessments.map((assessment) => ({
    id: getId(assessment, ["id", "assessmentId", "AssessmentId"]),
    claimId: getId(assessment, ["claimId", "ClaimId"]),
    evidenceId: getId(assessment, ["evidenceId", "EvidenceId"]),
    verdict: getText(assessment, ["verdict", "Verdict"]) || "Inconclusive",
    confidence:
      Number(getId(assessment, ["confidence", "confidenceScore", "ConfidenceScore"])) || 0,
    summary: getText(assessment, ["summary", "explanation", "Explanation"]),
  }));

  return {
    papers: normalizedPapers,
    claims: normalizedClaims,
    evidence: normalizedEvidence,
    assessments: normalizedAssessments,
    sources: normalizedSources,
  };
}

export default function Assistant() {
  const papers = useResource("Papers", mockPapers);
  const claims = useResource("Claims", mockClaims);
  const evidence = useResource("Evidence", mockEvidence);
  const assessments = useResource("ClaimAssessments", mockAssessments);
  const sources = useResource("Sources", mockSources);
  const [messages, setMessages] = useState(STARTER_MESSAGES);
  const [draft, setDraft] = useState("");
  const [sending, setSending] = useState(false);
  const [selectedPaperId, setSelectedPaperId] = useState("");
  const inputRef = useRef(null);

  const workspace = useMemo(
    () =>
      normalizeWorkspace({
        papers: papers.data,
        claims: claims.data,
        evidence: evidence.data,
        assessments: assessments.data,
        sources: sources.data,
      }),
    [papers.data, claims.data, evidence.data, assessments.data, sources.data]
  );

  const effectivePaperId = selectedPaperId || String(workspace.papers[0]?.id || "");
  const selectedPaper = workspace.papers.find((paper) => String(paper.id) === String(effectivePaperId));
  const paperClaims = workspace.claims.filter((claim) => String(claim.paperId) === String(effectivePaperId));
  const paperClaimIds = new Set(paperClaims.map((claim) => String(claim.id)));
  const paperEvidence = workspace.evidence.filter((item) => paperClaimIds.has(String(item.claimId)));
  const paperAssessments = workspace.assessments.filter((assessment) =>
    paperClaimIds.has(String(assessment.claimId))
  );
  const paperSourceIds = new Set(paperEvidence.map((item) => String(item.sourceId)));
  const paperSources = workspace.sources.filter((source) => paperSourceIds.has(String(source.id)));
  const paperContext = {
    paper: selectedPaper,
    claims: paperClaims,
    evidence: paperEvidence,
    assessments: paperAssessments,
    sources: paperSources,
  };

  const selectedAssessment = [...paperContext.assessments].sort(
    (a, b) => a.confidence - b.confidence
  )[0];
  const selectedClaim = paperContext.claims.find((claim) => claim.id === selectedAssessment?.claimId);
  const anyDemo =
    papers.isDemo || claims.isDemo || evidence.isDemo || assessments.isDemo || sources.isDemo;

  async function askAssistant(promptText = draft) {
    const message = promptText.trim();
    if (!message || sending) return;

    setDraft("");
    setSending(true);
    setMessages((current) => [...current, { role: "user", text: message }]);

    try {
      const response = await api.post("/assistant/chat", {
        message,
        paperId: anyDemo ? null : Number(effectivePaperId) || null,
        context: paperContext,
      });
      setMessages((current) => [
        ...current,
        { role: "assistant", text: response.data?.answer || "Groq did not return an answer." },
      ]);
    } catch (error) {
      const detail =
        error.response?.data?.detail ||
        error.response?.data?.title ||
        (error.code === "ECONNABORTED"
          ? "The assistant request timed out. The API or Groq took longer than 30 seconds to respond."
          : error.message
            ? `Assistant request failed: ${error.message}`
            : "Assistant request failed. Make sure the SourceLens API is running.");
      setMessages((current) => [
        ...current,
        { role: "assistant", text: detail },
      ]);
    } finally {
      setSending(false);
      inputRef.current?.focus();
    }
  }

  function submit(event) {
    event.preventDefault();
    askAssistant();
  }

  return (
    <div className="animate-fade-up">
      <Topbar eyebrow="Paper AI" title="Assistant">
        <select
          value={effectivePaperId}
          onChange={(event) => {
            setSelectedPaperId(event.target.value);
            setMessages(STARTER_MESSAGES);
          }}
          className="max-w-sm rounded-md border border-hairline bg-paper-raised px-3 py-2 text-sm text-ink outline-none focus:border-lens"
        >
          {workspace.papers.map((paper) => (
            <option key={paper.id} value={paper.id}>
              {paper.title}
            </option>
          ))}
        </select>
      </Topbar>
      {anyDemo && <DemoBanner />}

      <div className="grid min-h-[calc(100vh-180px)] grid-cols-1 gap-5 lg:grid-cols-[minmax(0,1fr)_320px]">
        <section className="flex min-h-[560px] flex-col overflow-hidden rounded-lg border border-hairline bg-paper-raised">
          <div className="flex-1 space-y-4 overflow-y-auto px-5 py-5">
            {messages.map((message, index) => (
              <div
                key={`${message.role}-${index}`}
                className={`flex ${message.role === "user" ? "justify-end" : "justify-start"}`}
              >
                <div
                  className={`max-w-[78%] whitespace-pre-line rounded-lg px-4 py-3 text-sm leading-relaxed ${
                    message.role === "user"
                      ? "bg-ink text-white"
                      : "border border-hairline bg-paper text-ink"
                  }`}
                >
                  {message.text}
                </div>
              </div>
            ))}
            {sending && (
              <div className="max-w-[78%] rounded-lg border border-hairline bg-paper px-4 py-3 text-sm text-ink-muted">
                Asking Groq about this paper...
              </div>
            )}
          </div>

          <div className="border-t border-hairline bg-paper/55 px-4 py-4">
            <div className="mb-3 flex flex-wrap gap-2">
              {QUICK_PROMPTS.map((prompt) => (
                <button
                  key={prompt}
                  type="button"
                  onClick={() => askAssistant(prompt)}
                  className="rounded-md border border-hairline bg-paper-raised px-3 py-1.5 text-xs font-medium text-ink-muted transition hover:border-lens-soft hover:text-ink"
                >
                  {prompt}
                </button>
              ))}
            </div>
            <form onSubmit={submit} className="flex gap-2">
              <textarea
                ref={inputRef}
                value={draft}
                onChange={(event) => setDraft(event.target.value)}
                onKeyDown={(event) => {
                  if (event.key === "Enter" && !event.shiftKey) {
                    event.preventDefault();
                    submit(event);
                  }
                }}
                rows={2}
                className="min-h-12 flex-1 resize-none rounded-md border border-hairline bg-white px-3 py-2 text-sm text-ink shadow-sm outline-none transition focus:border-lens"
                placeholder="Ask about the selected research paper"
              />
              <button
                type="submit"
                disabled={sending || !draft.trim()}
                className="h-12 rounded-md bg-lens px-5 text-sm font-semibold text-white transition hover:bg-lens-deep disabled:cursor-not-allowed disabled:opacity-60"
              >
                Send
              </button>
            </form>
          </div>
        </section>

        <aside className="space-y-4">
          <div className="rounded-lg border border-hairline bg-paper-raised p-4">
            <p className="text-xs font-semibold uppercase tracking-[0.12em] text-lens-deep">Selected Paper</p>
            <p className="mt-2 text-sm font-medium leading-snug text-ink">
              {selectedPaper?.title || "No paper selected"}
            </p>
            {selectedPaper?.authors && (
              <p className="mt-1 text-xs text-ink-muted">{selectedPaper.authors}</p>
            )}
            <div className="mt-4 grid grid-cols-2 gap-3 text-sm">
              <Metric label="Claims" value={paperContext.claims.length} />
              <Metric label="Evidence" value={paperContext.evidence.length} />
              <Metric label="Assessments" value={paperContext.assessments.length} />
              <Metric label="Sources" value={paperContext.sources.length} />
            </div>
          </div>

          {selectedAssessment && (
            <div className="rounded-lg border border-hairline bg-paper-raised p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="text-xs font-semibold uppercase tracking-[0.12em] text-lens-deep">
                    Lowest Confidence
                  </p>
                  <p className="mt-2 text-sm leading-snug text-ink">
                    {selectedClaim?.text || "Claim unavailable"}
                  </p>
                </div>
                <ConfidenceDial
                  confidence={selectedAssessment.confidence}
                  verdict={selectedAssessment.verdict}
                  size={46}
                />
              </div>
              <div className="mt-3">
                <VerdictBadge verdict={selectedAssessment.verdict} />
              </div>
              <p className="mt-3 text-xs leading-relaxed text-ink-muted">
                {selectedAssessment.summary || "No explanation recorded."}
              </p>
              {selectedPaper && (
                <p className="mt-3 border-t border-hairline pt-3 text-xs text-ink-muted">
                  {selectedPaper.title}
                </p>
              )}
            </div>
          )}

          <div className="rounded-lg border border-hairline bg-paper-raised p-4">
            <p className="text-xs font-semibold uppercase tracking-[0.12em] text-lens-deep">Recent Claims</p>
            <div className="mt-3 space-y-3">
              {paperContext.claims.slice(0, 4).map((claim) => (
                <button
                  key={claim.id}
                  type="button"
                  onClick={() => askAssistant(`Explain this claim: ${claim.text}`)}
                  className="w-full rounded-md border border-hairline bg-paper px-3 py-2 text-left text-xs leading-relaxed text-ink-muted transition hover:border-lens-soft hover:text-ink"
                >
                  {claim.text}
                </button>
              ))}
            </div>
          </div>
        </aside>
      </div>
    </div>
  );
}

function Metric({ label, value }) {
  return (
    <div className="rounded-md border border-hairline bg-paper px-3 py-2">
      <p className="font-display text-xl font-semibold text-ink">{value}</p>
      <p className="text-[11px] uppercase tracking-[0.1em] text-ink-muted">{label}</p>
    </div>
  );
}
