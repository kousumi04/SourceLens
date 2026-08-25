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

const EMPTY = { claimId: "", verdict: "Supported", confidence: 0.75, summary: "" };
const VERDICTS = ["Supported", "Refuted", "Inconclusive"];

export default function Assessments() {
  const assessments = useResource("ClaimAssessments", mockAssessments);
  const claims = useResource("Claims", mockClaims);
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState(EMPTY);
  const [editingId, setEditingId] = useState(null);

  const claimById = Object.fromEntries(claims.data.map((c) => [c.id, c]));

  const openNew = () => {
    setForm(EMPTY);
    setEditingId(null);
    setModalOpen(true);
  };
  const openEdit = (row) => {
    setForm(row);
    setEditingId(row.id);
    setModalOpen(true);
  };
  const submit = async () => {
    if (editingId) await assessments.update(editingId, form);
    else await assessments.create(form);
    setModalOpen(false);
  };

  const columns = [
    {
      key: "confidence",
      label: "",
      render: (r) => <ConfidenceDial confidence={r.confidence} verdict={r.verdict} size={44} />,
    },
    {
      key: "claimId",
      label: "Claim",
      render: (r) => (
        <div>
          <p className="max-w-md text-ink">{claimById[r.claimId]?.text || `Claim #${r.claimId}`}</p>
          <p className="mt-1 text-xs text-ink-muted">{r.summary}</p>
        </div>
      ),
    },
    { key: "verdict", label: "Verdict", render: (r) => <VerdictBadge verdict={r.verdict} /> },
  ];

  return (
    <div className="animate-fade-up">
      <Topbar eyebrow="Records" title="Claim Assessments">
        <button onClick={openNew} className="rounded-md bg-lens px-4 py-2 text-sm font-medium text-white transition hover:bg-lens-deep">
          + New assessment
        </button>
      </Topbar>
      {assessments.isDemo && <DemoBanner />}

      <DataTable
        columns={columns}
        rows={assessments.data}
        onEdit={openEdit}
        onDelete={(row) => assessments.remove(row.id)}
        onAddNew={openNew}
        emptyTitle="No claims assessed yet"
        emptyBody="Assess a claim against its linked evidence to record a verdict and confidence score."
      />

      <Modal open={modalOpen} title={editingId ? "Edit assessment" : "New assessment"} onClose={() => setModalOpen(false)}>
        <EntityForm
          value={form}
          onChange={setForm}
          onSubmit={submit}
          submitLabel={editingId ? "Save changes" : "Create assessment"}
          fields={[
            {
              key: "claimId",
              label: "Claim",
              type: "select",
              required: true,
              options: claims.data.map((c) => ({ value: c.id, label: c.text.slice(0, 60) })),
            },
            { key: "verdict", label: "Verdict", type: "select", options: VERDICTS.map((v) => ({ value: v, label: v })) },
            { key: "confidence", label: "Confidence", type: "range" },
            { key: "summary", label: "Summary", type: "textarea" },
          ]}
        />
      </Modal>
    </div>
  );
}
