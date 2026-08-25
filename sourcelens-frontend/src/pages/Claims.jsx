import { useState } from "react";
import Topbar from "../components/Topbar";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";
import EntityForm from "../components/EntityForm";
import DemoBanner from "../components/DemoBanner";
import { useResource } from "../hooks/useResource";
import { mockClaims, mockPapers } from "../api/mockData";

const EMPTY = { paperId: "", text: "", extractedFrom: "" };

export default function Claims() {
  const claims = useResource("Claims", mockClaims);
  const papers = useResource("Papers", mockPapers);
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState(EMPTY);
  const [editingId, setEditingId] = useState(null);

  const paperById = Object.fromEntries(papers.data.map((p) => [p.id, p]));

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
    if (editingId) await claims.update(editingId, form);
    else await claims.create(form);
    setModalOpen(false);
  };

  const columns = [
    { key: "text", label: "Claim", render: (r) => <p className="max-w-md text-ink">{r.text}</p> },
    { key: "paperId", label: "Paper", render: (r) => paperById[r.paperId]?.title || `Paper #${r.paperId}` },
    { key: "extractedFrom", label: "Section" },
  ];

  return (
    <div className="animate-fade-up">
      <Topbar eyebrow="Records" title="Claims">
        <button onClick={openNew} className="rounded-md bg-lens px-4 py-2 text-sm font-medium text-white transition hover:bg-lens-deep">
          + New claim
        </button>
      </Topbar>
      {claims.isDemo && <DemoBanner />}

      <DataTable
        columns={columns}
        rows={claims.data}
        onEdit={openEdit}
        onDelete={(row) => claims.remove(row.id)}
        onAddNew={openNew}
        emptyTitle="No claims extracted yet"
        emptyBody="Claims extracted from uploaded papers will appear here for verification."
      />

      <Modal open={modalOpen} title={editingId ? "Edit claim" : "New claim"} onClose={() => setModalOpen(false)}>
        <EntityForm
          value={form}
          onChange={setForm}
          onSubmit={submit}
          submitLabel={editingId ? "Save changes" : "Create claim"}
          fields={[
            {
              key: "paperId",
              label: "Paper",
              type: "select",
              required: true,
              options: papers.data.map((p) => ({ value: p.id, label: p.title })),
            },
            { key: "text", label: "Claim text", type: "textarea", required: true },
            { key: "extractedFrom", label: "Extracted from (section)" },
          ]}
        />
      </Modal>
    </div>
  );
}
