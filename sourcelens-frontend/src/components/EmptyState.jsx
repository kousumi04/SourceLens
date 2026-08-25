import Aperture from "./Aperture";

export default function EmptyState({ title = "Nothing recorded yet", body, actionLabel, onAction }) {
  return (
    <div className="flex flex-col items-center justify-center gap-3 rounded-lg border border-dashed border-hairline py-16 text-center">
      <Aperture size={40} color="var(--color-ink-soft)" open={0.3} />
      <p className="font-display text-lg text-ink">{title}</p>
      {body && <p className="max-w-sm text-sm text-ink-muted">{body}</p>}
      {actionLabel && (
        <button
          onClick={onAction}
          className="mt-2 rounded-md bg-ink px-4 py-2 text-sm font-medium text-paper transition hover:bg-ink-2"
        >
          {actionLabel}
        </button>
      )}
    </div>
  );
}
