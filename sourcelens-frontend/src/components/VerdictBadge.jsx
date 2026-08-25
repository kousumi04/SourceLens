const STYLES = {
  Supported: "text-support bg-support-soft",
  Refuted: "text-refute bg-refute-soft",
  Inconclusive: "text-pending bg-pending-soft",
};

export default function VerdictBadge({ verdict }) {
  const cls = STYLES[verdict] || STYLES.Inconclusive;
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold tracking-wide ${cls}`}
    >
      <span className="h-1.5 w-1.5 rounded-full bg-current" />
      {verdict}
    </span>
  );
}
