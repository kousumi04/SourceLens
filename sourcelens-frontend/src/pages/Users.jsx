import { useState } from "react";
import Topbar from "../components/Topbar";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";
import EntityForm from "../components/EntityForm";
import DemoBanner from "../components/DemoBanner";
import { useResource } from "../hooks/useResource";
import { mockUsers } from "../api/mockData";

const EMPTY = { name: "", email: "", role: "Researcher" };
const ROLES = ["Admin", "Researcher", "Reviewer"];

export default function Users() {
  const users = useResource("Users", mockUsers);
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
    if (editingId) await users.update(editingId, form);
    else await users.create(form);
    setModalOpen(false);
  };

  const columns = [
    {
      key: "name",
      label: "Name",
      render: (r) => (
        <div className="flex items-center gap-2.5">
          <span className="flex h-7 w-7 items-center justify-center rounded-full bg-lens-soft font-display text-xs font-semibold text-lens-deep">
            {r.name?.split(" ").map((n) => n[0]).slice(0, 2).join("")}
          </span>
          {r.name}
        </div>
      ),
    },
    { key: "email", label: "Email", mono: true },
    { key: "role", label: "Role" },
  ];

  return (
    <div className="animate-fade-up">
      <Topbar eyebrow="Team" title="Users">
        <button onClick={openNew} className="rounded-md bg-lens px-4 py-2 text-sm font-medium text-white transition hover:bg-lens-deep">
          + New user
        </button>
      </Topbar>
      {users.isDemo && <DemoBanner />}

      <DataTable
        columns={columns}
        rows={users.data}
        onEdit={openEdit}
        onDelete={(row) => users.remove(row.id)}
        onAddNew={openNew}
        emptyTitle="No users yet"
        emptyBody="Add teammates who can upload papers and review claims."
      />

      <Modal open={modalOpen} title={editingId ? "Edit user" : "New user"} onClose={() => setModalOpen(false)}>
        <EntityForm
          value={form}
          onChange={setForm}
          onSubmit={submit}
          submitLabel={editingId ? "Save changes" : "Create user"}
          fields={[
            { key: "name", label: "Name", required: true },
            { key: "email", label: "Email", type: "email", required: true },
            { key: "role", label: "Role", type: "select", options: ROLES.map((r) => ({ value: r, label: r })) },
          ]}
        />
      </Modal>
    </div>
  );
}
