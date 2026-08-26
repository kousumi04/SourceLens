import { useState } from "react";
import Topbar from "../components/Topbar";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";
import EntityForm from "../components/EntityForm";
import DemoBanner from "../components/DemoBanner";
import VerdictBadge from "../components/VerdictBadge";
import ConfidenceDial from "../components/ConfidenceDial";
import { useResource } from "../hooks/useResource";
import { mockAssessments, mockClaims } from "../api/mockData";

const EMPTY = {
  claimId: "",
  evidenceId: "",
  verdict: "Supported",
  confidenceScore: 0.75,
  explanation: "",
};

const VERDICTS = ["Supported", "Refuted", "Inconclusive"];

export default function Assessments() {
  const assessments = useResource(
    "ClaimAssessments",
    mockAssessments
  );

  const claims = useResource("Claims", mockClaims);

  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState(EMPTY);
  const [editingId, setEditingId] = useState(null);

  const claimById = Object.fromEntries(
    claims.data.map((c) => [
      c.claimId ?? c.id,
      c,
    ])
  );

  const openNew = () => {
    setForm({
      claimId: "",
      evidenceId: "",
      verdict: "Supported",
      confidenceScore: 0.75,
      explanation: "",
    });

    setEditingId(null);
    setModalOpen(true);
  };

  const openEdit = (row) => {
    setForm({
      claimId: row.claimId ?? "",
      evidenceId: row.evidenceId ?? "",
      verdict: row.verdict ?? "Supported",
      confidenceScore: row.confidenceScore ?? 0.75,
      explanation: row.explanation ?? "",
    });

    setEditingId(row.assessmentId);
    setModalOpen(true);
  };

  const submit = async () => {
    const payload = {
      claimId: Number(form.claimId),
      evidenceId: Number(form.evidenceId),
      verdict: form.verdict,
      confidenceScore: Number(form.confidenceScore),
      explanation: form.explanation,
    };

    if (editingId) {
      await assessments.update(editingId, payload);
    } else {
      await assessments.create(payload);
    }

    setModalOpen(false);
  };

  const columns = [
    {
      key: "confidenceScore",
      label: "",
      render: (r) => (
        <ConfidenceDial
          confidence={r.confidenceScore}
          verdict={r.verdict}
          size={44}
        />
      ),
    },

    {
      key: "claimId",
      label: "Claim",
      render: (r) => (
        <div>
          <p className="max-w-md text-ink">
            {claimById[r.claimId]?.claimText ||
              claimById[r.claimId]?.text ||
              `Claim #${r.claimId}`}
          </p>

          <p className="mt-1 text-xs text-ink-muted">
            {r.explanation}
          </p>
        </div>
      ),
    },

    {
      key: "evidenceId",
      label: "Evidence",
      render: (r) => (
        <span className="text-ink-muted">
          Evidence #{r.evidenceId}
        </span>
      ),
    },

    {
      key: "verdict",
      label: "Verdict",
      render: (r) => (
        <VerdictBadge verdict={r.verdict} />
      ),
    },

    {
      key: "confidenceScore",
      label: "Confidence",
      render: (r) => (
        <span className="font-mono text-sm">
          {Math.round((r.confidenceScore ?? 0) * 100)}%
        </span>
      ),
    },
  ];

  return (
    <div className="animate-fade-up">

      <Topbar
        eyebrow="Records"
        title="Claim Assessments"
      >
        <button
          onClick={openNew}
          className="rounded-md bg-lens px-4 py-2 text-sm font-medium text-white transition hover:bg-lens-deep"
        >
          + New assessment
        </button>
      </Topbar>

      {assessments.isDemo && <DemoBanner />}

      <DataTable
        columns={columns}
        rows={assessments.data}
        onEdit={openEdit}
        onDelete={(row) =>
          assessments.remove(row.assessmentId)
        }
        onAddNew={openNew}
        emptyTitle="No claims assessed yet"
        emptyBody="Assess a claim against its linked evidence to record a verdict and confidence score."
      />

      <Modal
        open={modalOpen}
        title={
          editingId
            ? "Edit assessment"
            : "New assessment"
        }
        onClose={() => setModalOpen(false)}
      >

        <EntityForm
          value={form}
          onChange={setForm}
          onSubmit={submit}
          submitLabel={
            editingId
              ? "Save changes"
              : "Create assessment"
          }

          fields={[
            {
              key: "claimId",
              label: "Claim",
              type: "select",
              required: true,

              options: claims.data.map((c) => ({
                value: c.claimId ?? c.id,
                label: (
                  c.claimText ??
                  c.text ??
                  `Claim #${c.claimId ?? c.id}`
                ).slice(0, 60),
              })),
            },

            {
              key: "evidenceId",
              label: "Evidence ID",
              type: "number",
              required: true,
            },

            {
              key: "verdict",
              label: "Verdict",
              type: "select",

              options: VERDICTS.map((v) => ({
                value: v,
                label: v,
              })),
            },

            {
              key: "confidenceScore",
              label: "Confidence",
              type: "range",
            },

            {
              key: "explanation",
              label: "Explanation",
              type: "textarea",
            },
          ]}
        />

      </Modal>

    </div>
  );
}