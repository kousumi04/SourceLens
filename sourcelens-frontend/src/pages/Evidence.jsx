import { useState } from "react";
import Topbar from "../components/Topbar";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";
import EntityForm from "../components/EntityForm";
import DemoBanner from "../components/DemoBanner";
import { useResource } from "../hooks/useResource";
import { mockEvidence, mockSources } from "../api/mockData";

const EMPTY = {
  sourceId: "",
  evidenceText: "",
  pageNumber: "",
};

export default function Evidence() {
  const evidence = useResource("Evidence", mockEvidence);
  const sources = useResource("Sources", mockSources);

  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState(EMPTY);
  const [editingId, setEditingId] = useState(null);

  const sourceById = Object.fromEntries(
    sources.data.map((s) => [s.sourceId ?? s.id, s])
  );

  const openNew = () => {
    setForm(EMPTY);
    setEditingId(null);
    setModalOpen(true);
  };

  const openEdit = (row) => {
    setForm({
      sourceId: row.sourceId ?? "",
      evidenceText: row.evidenceText ?? row.text ?? "",
      pageNumber: row.pageNumber ?? "",
    });

    setEditingId(row.evidenceId ?? row.id);
    setModalOpen(true);
  };

  const submit = async () => {
    const payload = {
      sourceId: Number(form.sourceId),
      evidenceText: form.evidenceText,
      pageNumber: Number(form.pageNumber),
    };

    if (editingId) {
      await evidence.update(editingId, payload);
    } else {
      await evidence.create(payload);
    }

    setModalOpen(false);
    setForm(EMPTY);
    setEditingId(null);
  };

  const columns = [
    {
      key: "evidenceText",
      label: "Evidence",
      render: (r) => (
        <p className="max-w-lg text-ink">
          {r.evidenceText ?? r.text ?? "—"}
        </p>
      ),
    },

    {
      key: "sourceId",
      label: "Source",
      render: (r) =>
        sourceById[r.sourceId]?.title || `Source #${r.sourceId}`,
    },

    {
      key: "pageNumber",
      label: "Page",
      render: (r) => (
        <span className="font-mono text-sm">
          {r.pageNumber ?? "—"}
        </span>
      ),
    },

    {
      key: "evidenceId",
      label: "ID",
      render: (r) => (
        <span className="font-mono text-sm text-ink-muted">
          #{r.evidenceId ?? r.id}
        </span>
      ),
    },
  ];

  return (
    <div className="animate-fade-up">
      <Topbar eyebrow="Records" title="Evidence">
        <button
          onClick={openNew}
          className="rounded-md bg-lens px-4 py-2 text-sm font-medium text-white transition hover:bg-lens-deep"
        >
          + New evidence
        </button>
      </Topbar>

      {evidence.isDemo && <DemoBanner />}

      <DataTable
        columns={columns}
        rows={evidence.data}
        onEdit={openEdit}
        onDelete={(row) =>
          evidence.remove(row.evidenceId ?? row.id)
        }
        onAddNew={openNew}
        emptyTitle="No evidence linked yet"
        emptyBody="Retrieved evidence from sources will appear here."
      />

      <Modal
        open={modalOpen}
        title={editingId ? "Edit evidence" : "New evidence"}
        onClose={() => setModalOpen(false)}
      >
        <EntityForm
          value={form}
          onChange={setForm}
          onSubmit={submit}
          submitLabel={editingId ? "Save changes" : "Create evidence"}
          fields={[
            {
              key: "sourceId",
              label: "Source",
              type: "select",
              required: true,
              options: sources.data.map((s) => ({
                value: s.sourceId ?? s.id,
                label: s.title,
              })),
            },

            {
              key: "evidenceText",
              label: "Evidence text",
              type: "textarea",
              required: true,
            },

            {
              key: "pageNumber",
              label: "Page number",
              type: "number",
              required: true,
            },
          ]}
        />
      </Modal>
    </div>
  );
}