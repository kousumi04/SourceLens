import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from "recharts";
import Topbar from "../components/Topbar";
import StatCard from "../components/StatCard";
import VerdictBadge from "../components/VerdictBadge";
import ConfidenceDial from "../components/ConfidenceDial";
import DemoBanner from "../components/DemoBanner";
import { useResource } from "../hooks/useResource";
import { api } from "../api/client";
import { useState } from "react";
import {
  mockPapers,
  mockClaims,
  mockEvidence,
  mockAssessments,
} from "../api/mockData";

const VERDICT_COLORS = {
  Supported: "#2f6f4e",
  Refuted: "#9c4a3a",
  Inconclusive: "#b98a2e",
};

export default function Dashboard() {
  const [summary, setSummary] = useState("");
  const [summarizing, setSummarizing] = useState(false);
  const [summaryError, setSummaryError] = useState("");
  const papers = useResource("Papers", mockPapers);
  const claims = useResource("Claims", mockClaims);
  const evidence = useResource("Evidence", mockEvidence);
  const assessments = useResource("ClaimAssessments", mockAssessments);

  const anyDemo = papers.isDemo || claims.isDemo || evidence.isDemo || assessments.isDemo;

  const verdictCounts = ["Supported", "Refuted", "Inconclusive"].map((v) => ({
    name: v,
    value: assessments.data.filter((a) => a.verdict === v).length,
  }));
  const totalAssessed = verdictCounts.reduce((s, v) => s + v.value, 0);

  const avgConfidence =
    assessments.data.length > 0
      ? assessments.data.reduce((s, a) => s + (a.confidence || 0), 0) / assessments.data.length
      : 0;

  const claimById = Object.fromEntries(claims.data.map((c) => [c.id, c]));
  const paperById = Object.fromEntries(papers.data.map((p) => [p.id, p]));
  const evidenceCountByClaim = claims.data.reduce((acc, c) => {
    acc[c.id] = evidence.data.filter((e) => e.claimId === c.id).length;
    return acc;
  }, {});

  const recentAssessments = [...assessments.data].slice(-4).reverse();

  async function summarizeDashboard() {
    setSummarizing(true); setSummaryError("");
    try {
      const result = await api.post("/dashboard/summary", {
        papers: papers.data.length, claims: claims.data.length, evidenceLinked: evidence.data.length,
        assessedClaims: totalAssessed, averageConfidence: avgConfidence,
        verdictDistribution: verdictCounts,
        recentClaims: recentAssessments.map((a) => ({ claim: claimById[a.claimId]?.text, verdict: a.verdict, confidence: a.confidence }))
      });
      setSummary(result.data.summary || "No summary was returned.");
    } catch (error) { setSummaryError(error.response?.data?.detail || "Could not create the summary. Check the backend and Groq key."); }
    finally { setSummarizing(false); }
  }

  return (
    <div className="animate-fade-up">
      <Topbar eyebrow="Overview" title="Dashboard">
        <button onClick={summarizeDashboard} disabled={summarizing} className="rounded-md bg-ink px-3.5 py-2 text-sm font-semibold text-white transition hover:bg-ink-2 disabled:cursor-wait disabled:opacity-60">
          {summarizing ? "Summarizing…" : "Summarize with AI"}
        </button>
      </Topbar>
      {(summary || summaryError) && <div className="mb-5 rounded-lg border border-lens-soft bg-lens-soft/40 px-4 py-3 text-sm leading-relaxed text-ink"><p className="mb-1 text-xs font-semibold uppercase tracking-wide text-lens-deep">AI summary</p>{summary || summaryError}</div>}
      {anyDemo && <DemoBanner />}

      <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
        <StatCard label="Papers" value={papers.data.length} sublabel="Uploaded to SourceLens" />
        <StatCard label="Claims" value={claims.data.length} sublabel="Extracted for review" />
        <StatCard label="Evidence linked" value={evidence.data.length} sublabel="Across all claims" />
        <StatCard
          label="Avg. confidence"
          value={`${Math.round(avgConfidence * 100)}%`}
          sublabel={`${totalAssessed} claims assessed`}
        />
      </div>

      <div className="mt-6 grid grid-cols-1 gap-5 lg:grid-cols-5">
        {/* Verdict distribution */}
        <div className="rounded-lg border border-hairline bg-paper-raised p-5 lg:col-span-2">
          <p className="mb-1 font-display text-base font-semibold text-ink">Verdict distribution</p>
          <p className="mb-3 text-xs text-ink-muted">How assessed claims have resolved so far</p>
          <div className="relative h-56">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={verdictCounts}
                  dataKey="value"
                  nameKey="name"
                  innerRadius={55}
                  outerRadius={85}
                  paddingAngle={3}
                  stroke="none"
                >
                  {verdictCounts.map((entry) => (
                    <Cell key={entry.name} fill={VERDICT_COLORS[entry.name]} />
                  ))}
                </Pie>
                <Tooltip
                  contentStyle={{
                    borderRadius: 8,
                    border: "1px solid var(--color-hairline)",
                    fontFamily: "var(--font-body)",
                    fontSize: 12,
                  }}
                />
              </PieChart>
            </ResponsiveContainer>
            <div className="pointer-events-none absolute inset-0 flex flex-col items-center justify-center">
              <span className="font-display text-2xl font-semibold text-ink">{totalAssessed}</span>
              <span className="text-[11px] uppercase tracking-wide text-ink-muted">assessed</span>
            </div>
          </div>
          <div className="mt-2 flex justify-center gap-4">
            {verdictCounts.map((v) => (
              <div key={v.name} className="flex items-center gap-1.5 text-xs text-ink-muted">
                <span className="h-2 w-2 rounded-full" style={{ background: VERDICT_COLORS[v.name] }} />
                {v.name} ({v.value})
              </div>
            ))}
          </div>
        </div>

        {/* Recent evidence trail */}
        <div className="rounded-lg border border-hairline bg-paper-raised p-5 lg:col-span-3">
          <p className="mb-1 font-display text-base font-semibold text-ink">Recent claim trail</p>
          <p className="mb-4 text-xs text-ink-muted">
            Claim → evidence → verdict, most recently assessed first
          </p>
          <div className="flex flex-col gap-5">
            {recentAssessments.map((a, i) => {
              const claim = claimById[a.claimId];
              const paper = claim ? paperById[claim.paperId] : null;
              return (
                <div key={a.id} className="relative flex gap-4 pl-1">
                  <div className="flex flex-col items-center">
                    <ConfidenceDial confidence={a.confidence} verdict={a.verdict} size={44} />
                    {i < recentAssessments.length - 1 && <div className="mt-1 h-full w-px flex-1 bg-hairline" />}
                  </div>
                  <div className="pb-1">
                    <p className="text-sm leading-snug text-ink">{claim?.text || "Claim unavailable"}</p>
                    <p className="mt-1 text-xs text-ink-muted">
                      {paper?.title || "Unknown paper"} · {evidenceCountByClaim[a.claimId] ?? 0} evidence linked
                    </p>
                    <div className="mt-1.5">
                      <VerdictBadge verdict={a.verdict} />
                    </div>
                  </div>
                </div>
              );
            })}
            {recentAssessments.length === 0 && (
              <p className="text-sm text-ink-muted">No claim assessments recorded yet.</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
