// fields: [{ key, label, type: 'text'|'number'|'textarea'|'select', options?, step? }]
export default function EntityForm({ fields, value, onChange, onSubmit, submitLabel = "Save" }) {
  const set = (key, v) => onChange({ ...value, [key]: v });

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault();
        onSubmit();
      }}
      className="flex flex-col gap-4"
    >
      {fields.map((f) => (
        <label key={f.key} className="flex flex-col gap-1.5">
          <span className="text-xs font-semibold uppercase tracking-wide text-ink-muted">{f.label}</span>
          {f.type === "textarea" ? (
            <textarea
              required={f.required}
              rows={3}
              value={value[f.key] ?? ""}
              onChange={(e) => set(f.key, e.target.value)}
              className="rounded-md border border-hairline bg-paper px-3 py-2 text-sm text-ink outline-none focus:border-lens"
            />
          ) : f.type === "select" ? (
            <select
              required={f.required}
              value={value[f.key] ?? ""}
              onChange={(e) => set(f.key, e.target.value)}
              className="rounded-md border border-hairline bg-paper px-3 py-2 text-sm text-ink outline-none focus:border-lens"
            >
              <option value="" disabled>
                Select…
              </option>
              {f.options.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
          ) : f.type === "range" ? (
            <div className="flex items-center gap-3">
              <input
                type="range"
                min={0}
                max={1}
                step={0.01}
                value={value[f.key] ?? 0}
                onChange={(e) => set(f.key, parseFloat(e.target.value))}
                className="flex-1 accent-[#B8933E]"
              />
              <span className="w-12 text-right font-mono text-sm text-ink">
                {Math.round((value[f.key] ?? 0) * 100)}%
              </span>
            </div>
          ) : (
            <input
              required={f.required}
              type={f.type || "text"}
              step={f.step}
              value={value[f.key] ?? ""}
              onChange={(e) =>
                set(f.key, f.type === "number" ? e.target.valueAsNumber || 0 : e.target.value)
              }
              className="rounded-md border border-hairline bg-paper px-3 py-2 text-sm text-ink outline-none focus:border-lens"
            />
          )}
        </label>
      ))}

      <button
        type="submit"
        className="mt-2 self-start rounded-md bg-ink px-4 py-2 text-sm font-medium text-paper transition hover:bg-ink-2"
      >
        {submitLabel}
      </button>
    </form>
  );
}
