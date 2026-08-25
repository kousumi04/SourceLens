const VERDICT_COLOR = {
  Supported: "var(--color-support)",
  Refuted: "var(--color-refute)",
  Inconclusive: "var(--color-pending)",
};

export default function ConfidenceDial({ confidence = 0, verdict = "Inconclusive", size = 56 }) {
  const pct = Math.max(0, Math.min(1, confidence));
  const r = 40;
  const c = 2 * Math.PI * r;
  const dash = c * pct;
  const color = VERDICT_COLOR[verdict] || VERDICT_COLOR.Inconclusive;

  return (
    <div className="relative inline-flex items-center justify-center" style={{ width: size, height: size }}>
      <svg width={size} height={size} viewBox="0 0 100 100" className="-rotate-90">
        <circle cx="50" cy="50" r={r} fill="none" stroke="var(--color-hairline)" strokeWidth="10" />
        <circle
          cx="50"
          cy="50"
          r={r}
          fill="none"
          stroke={color}
          strokeWidth="10"
          strokeLinecap="round"
          strokeDasharray={`${dash} ${c - dash}`}
        />
      </svg>
      <span className="absolute font-mono text-[11px] font-semibold" style={{ color }}>
        {Math.round(pct * 100)}%
      </span>
    </div>
  );
}
