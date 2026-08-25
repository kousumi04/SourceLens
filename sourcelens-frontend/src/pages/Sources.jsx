import { useState } from "react";
import Topbar from "../components/Topbar";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";
import EntityForm from "../components/EntityForm";
import DemoBanner from "../components/DemoBanner";
import { useResource } from "../hooks/useResource";
import { mockSources } from "../api/mockData";

const EMPTY = { title: "", url: "", type: "Journal Article" };
const TYPES = ["Journal Article", "Dataset", "Preprint", "Report", "Book", "Website"];

export default function Sources() {
  const sources = useResource("Sources", mockSources);
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState(EMPTY);
  const [editingId, setEditingId] = useState(null);

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
    if (editingId) await sources.update(editingId, form);
    else await sources.create(form);
    setModalOpen(false);
  };

  const columns = [
    { key: "title", label: "Source" },
    {
      key: "url",
      label: "URL",
      mono: true,
      render: (r) => (
        <a href={r.url} target="_blank" rel="noreferrer" className="text-lens-deep hover:underline">
          {r.url}
        </a>
      ),
    },
    { key: "type", label: "Type" },
  ];

  return (
    <div className="animate-fade-up">
      <Topbar eyebrow="Records" title="Sources">
        <button onClick={openNew} className="rounded-md bg-lens px-4 py-2 text-sm font-medium text-white transition hover:bg-lens-deep">
          + New source
        </button>
      </Topbar>
      {sources.isDemo && <DemoBanner />}

      <DataTable
        columns={columns}
        rows={sources.data}
        onEdit={openEdit}
        onDelete={(row) => sources.remove(row.id)}
        onAddNew={openNew}
        emptyTitle="No sources catalogued yet"
        emptyBody="Sources used to support or refute claims will appear here."
      />

      <Modal open={modalOpen} title={editingId ? "Edit source" : "New source"} onClose={() => setModalOpen(false)}>
        <EntityForm
          value={form}
          onChange={setForm}
          onSubmit={submit}
          submitLabel={editingId ? "Save changes" : "Create source"}
          fields={[
            { key: "title", label: "Title", required: true },
            { key: "url", label: "URL", required: true },
            { key: "type", label: "Type", type: "select", options: TYPES.map((t) => ({ value: t, label: t })) },
          ]}
        />
      </Modal>
    </div>
  );
}
