import { useState } from "react";
import Topbar from "../components/Topbar";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";
import EntityForm from "../components/EntityForm";
import DemoBanner from "../components/DemoBanner";
import { useResource } from "../hooks/useResource";
import { mockPapers, mockUsers } from "../api/mockData";

const EMPTY = { title: "", authors: "", year: new Date().getFullYear(), journal: "", doi: "", uploadedBy: "" };

export default function Papers() {
  const papers = useResource("Papers", mockPapers);
  const users = useResource("Users", mockUsers);
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState(EMPTY);
  const [editingId, setEditingId] = useState(null);

  const userById = Object.fromEntries(users.data.map((u) => [u.id, u]));

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
    if (editingId) await papers.update(editingId, form);
    else await papers.create({ ...form, uploadedDate: new Date().toISOString().slice(0, 10) });
    setModalOpen(false);
  };

  const columns = [
    {
      key: "title",
      label: "Title",
      render: (r) => (
        <div>
          <p className="font-display text-[15px] leading-snug text-ink">{r.title}</p>
          <p className="mt-0.5 text-xs text-ink-muted">{r.authors}</p>
        </div>
      ),
    },
    { key: "journal", label: "Journal" },
    { key: "year", label: "Year", mono: true },
    { key: "doi", label: "DOI", mono: true },
    { key: "uploadedBy", label: "Uploaded by", render: (r) => userById[r.uploadedBy]?.name || `User #${r.uploadedBy}` },
  ];

  return (
    <div className="animate-fade-up">
      <Topbar eyebrow="Records" title="Research Papers">
        <button onClick={openNew} className="rounded-md bg-lens px-4 py-2 text-sm font-medium text-white transition hover:bg-lens-deep">
          + New paper
        </button>
      </Topbar>
      {papers.isDemo && <DemoBanner />}

      <DataTable
        columns={columns}
        rows={papers.data}
        onEdit={openEdit}
        onDelete={(row) => papers.remove(row.id)}
        onAddNew={openNew}
        emptyTitle="No papers uploaded yet"
        emptyBody="Upload a research paper to begin extracting claims from it."
      />

      <Modal open={modalOpen} title={editingId ? "Edit paper" : "New paper"} onClose={() => setModalOpen(false)}>
        <EntityForm
          value={form}
          onChange={setForm}
          onSubmit={submit}
          submitLabel={editingId ? "Save changes" : "Create paper"}
          fields={[
            { key: "title", label: "Title", required: true },
            { key: "authors", label: "Authors", required: true },
            { key: "journal", label: "Journal / venue" },
            { key: "year", label: "Year", type: "number" },
            { key: "doi", label: "DOI / URL" },
            {
              key: "uploadedBy",
              label: "Uploaded by",
              type: "select",
              options: users.data.map((u) => ({ value: u.id, label: u.name })),
            },
          ]}
        />
      </Modal>
    </div>
  );
}
