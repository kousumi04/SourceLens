import EmptyState from "./EmptyState";

// columns: [{ key, label, render?: (row) => node, mono?: bool }]
export default function DataTable({ columns, rows, onEdit, onDelete, emptyTitle, emptyBody, onAddNew }) {
  if (!rows.length) {
    return <EmptyState title={emptyTitle} body={emptyBody} actionLabel={onAddNew ? "Add new" : undefined} onAction={onAddNew} />;
  }

  return (
    <div className="overflow-hidden rounded-lg border border-hairline bg-paper-raised">
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="border-b border-hairline bg-paper/60 text-left">
            {columns.map((c) => (
              <th key={c.key} className="px-4 py-3 text-[11px] font-semibold uppercase tracking-[0.1em] text-ink-muted">
                {c.label}
              </th>
            ))}
            {(onEdit || onDelete) && <th className="px-4 py-3" />}
          </tr>
        </thead>
        <tbody>
          {rows.map((row, i) => (
            <tr
              key={row.id}
              className={`border-b border-hairline last:border-0 hover:bg-paper/50 ${i % 2 === 1 ? "bg-paper/25" : ""}`}
            >
              {columns.map((c) => (
                <td key={c.key} className={`px-4 py-3 align-top text-ink ${c.mono ? "font-mono text-xs" : ""}`}>
                  {c.render ? c.render(row) : row[c.key]}
                </td>
              ))}
              {(onEdit || onDelete) && (
                <td className="whitespace-nowrap px-4 py-3 text-right">
                  {onEdit && (
                    <button
                      onClick={() => onEdit(row)}
                      className="mr-3 text-xs font-medium text-ink-muted hover:text-lens-deep"
                    >
                      Edit
                    </button>
                  )}
                  {onDelete && (
                    <button
                      onClick={() => onDelete(row)}
                      className="text-xs font-medium text-ink-muted hover:text-refute"
                    >
                      Delete
                    </button>
                  )}
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
