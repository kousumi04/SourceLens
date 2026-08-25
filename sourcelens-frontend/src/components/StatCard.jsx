export default function StatCard({ label, value, sublabel }) {
  return (
    <div className="rounded-lg border border-hairline bg-paper-raised px-5 py-4">
      <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-ink-muted">{label}</p>
      <p className="mt-1.5 font-display text-3xl font-semibold text-ink">{value}</p>
      {sublabel && <p className="mt-1 text-xs text-ink-muted">{sublabel}</p>}
    </div>
  );
}
