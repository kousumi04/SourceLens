import { useState } from "react";
import Topbar from "../components/Topbar";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";
import EntityForm from "../components/EntityForm";
import DemoBanner from "../components/DemoBanner";
import { useResource } from "../hooks/useResource";
import { mockClaims, mockPapers } from "../api/mockData";

const EMPTY = {
  paperId: "",
  claimText: "",
  pageNumber: "",
};

export default function Claims() {
  const claims = useResource("Claims", mockClaims);
  const papers = useResource("Papers", mockPapers);

  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState(EMPTY);
  const [editingId, setEditingId] = useState(null);

  const paperById = Object.fromEntries(
    papers.data.map((p) => [p.paperId ?? p.id, p])
  );

  const openNew = () => {
    setForm(EMPTY);
    setEditingId(null);
    setModalOpen(true);
  };

  const openEdit = (row) => {
    setForm({
      paperId: row.paperId ?? "",
      claimText: row.claimText ?? row.text ?? "",
      pageNumber: row.pageNumber ?? "",
    });

    setEditingId(row.claimId ?? row.id);
    setModalOpen(true);
  };

  const submit = async () => {
    const payload = {
      paperId: Number(form.paperId),
      claimText: form.claimText,
      pageNumber: Number(form.pageNumber),
    };

    if (editingId) {
      await claims.update(editingId, payload);
    } else {
      await claims.create(payload);
    }

    setModalOpen(false);
    setForm(EMPTY);
    setEditingId(null);
  };

  const columns = [
    {
      key: "claimText",
      label: "Claim",
      render: (r) => (
        <p className="max-w-lg text-ink">
          {r.claimText ?? r.text ?? "—"}
        </p>
      ),
    },

    {
      key: "paperId",
      label: "Paper",
      render: (r) => {
        const paper = paperById[r.paperId];

        return (
          <p className="max-w-[220px] truncate text-ink-muted">
            {paper?.title || `Paper #${r.paperId}`}
          </p>
        );
      },
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
      key: "claimId",
      label: "ID",
      render: (r) => (
        <span className="font-mono text-sm text-ink-muted">
          #{r.claimId ?? r.id}
        </span>
      ),
    },
  ];

  return (
    <div className="animate-fade-up">
      <Topbar eyebrow="Records" title="Claims">
        <button
          onClick={openNew}
          className="rounded-md bg-lens px-4 py-2 text-sm font-medium text-white transition hover:bg-lens-deep"
        >
          + New claim
        </button>
      </Topbar>

      {claims.isDemo && <DemoBanner />}

      <DataTable
        columns={columns}
        rows={claims.data}
        onEdit={openEdit}
        onDelete={(row) =>
          claims.remove(row.claimId ?? row.id)
        }
        onAddNew={openNew}
        emptyTitle="No claims extracted yet"
        emptyBody="Claims extracted from uploaded papers will appear here for verification."
      />

      <Modal
        open={modalOpen}
        title={editingId ? "Edit claim" : "New claim"}
        onClose={() => setModalOpen(false)}
      >
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
              options: papers.data.map((p) => ({
                value: p.paperId ?? p.id,
                label: p.title ?? p.fileName,
              })),
            },

            {
              key: "claimText",
              label: "Claim text",
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