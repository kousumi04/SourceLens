import { useState } from "react";
import Topbar from "../components/Topbar";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";
import EntityForm from "../components/EntityForm";
import DemoBanner from "../components/DemoBanner";
import { useResource } from "../hooks/useResource";
import { mockEvidence, mockClaims, mockSources } from "../api/mockData";

const EMPTY = { claimId: "", sourceId: "", text: "", supportType: "Supports" };
const SUPPORT_TYPES = ["Supports", "Refutes", "Neutral"];

const SUPPORT_STYLE = {
  Supports: "text-support bg-support-soft",
  Refutes: "text-refute bg-refute-soft",
  Neutral: "text-pending bg-pending-soft",
};

export default function Evidence() {
  const evidence = useResource("Evidence", mockEvidence);
  const claims = useResource("Claims", mockClaims);
  const sources = useResource("Sources", mockSources);
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState(EMPTY);
  const [editingId, setEditingId] = useState(null);

  const claimById = Object.fromEntries(claims.data.map((c) => [c.id, c]));
  const sourceById = Object.fromEntries(sources.data.map((s) => [s.id, s]));

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
    if (editingId) await evidence.update(editingId, form);
    else await evidence.create(form);
    setModalOpen(false);
  };

  const columns = [
    { key: "text", label: "Evidence", render: (r) => <p className="max-w-md text-ink">{r.text}</p> },
    { key: "claimId", label: "Claim", render: (r) => <p className="max-w-[220px] truncate text-ink-muted">{claimById[r.claimId]?.text || `#${r.claimId}`}</p> },
    { key: "sourceId", label: "Source", render: (r) => sourceById[r.sourceId]?.title || `#${r.sourceId}` },
    {
      key: "supportType",
      label: "Relation",
      render: (r) => (
        <span className={`rounded-full px-2.5 py-1 text-xs font-semibold ${SUPPORT_STYLE[r.supportType]}`}>
          {r.supportType}
        </span>
      ),
    },
  ];

  return (
    <div className="animate-fade-up">
      <Topbar eyebrow="Records" title="Evidence">
        <button onClick={openNew} className="rounded-md bg-lens px-4 py-2 text-sm font-medium text-white transition hover:bg-lens-deep">
          + New evidence
        </button>
      </Topbar>
      {evidence.isDemo && <DemoBanner />}

      <DataTable
        columns={columns}
        rows={evidence.data}
        onEdit={openEdit}
        onDelete={(row) => evidence.remove(row.id)}
        onAddNew={openNew}
        emptyTitle="No evidence linked yet"
        emptyBody="Link supporting or refuting evidence from sources to a claim."
      />

      <Modal open={modalOpen} title={editingId ? "Edit evidence" : "New evidence"} onClose={() => setModalOpen(false)}>
        <EntityForm
          value={form}
          onChange={setForm}
          onSubmit={submit}
          submitLabel={editingId ? "Save changes" : "Create evidence"}
          fields={[
            {
              key: "claimId",
              label: "Claim",
              type: "select",
              required: true,
              options: claims.data.map((c) => ({ value: c.id, label: c.text.slice(0, 60) })),
            },
            {
              key: "sourceId",
              label: "Source",
              type: "select",
              required: true,
              options: sources.data.map((s) => ({ value: s.id, label: s.title })),
            },
            { key: "text", label: "Evidence text", type: "textarea", required: true },
            {
              key: "supportType",
              label: "Relation to claim",
              type: "select",
              options: SUPPORT_TYPES.map((t) => ({ value: t, label: t })),
            },
          ]}
        />
      </Modal>
    </div>
  );
}
