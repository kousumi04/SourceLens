import { API_BASE_URL } from "../api/client";

export default function DemoBanner() {
  return (
    <div className="mb-5 flex items-start gap-3 rounded-lg border border-lens-soft bg-lens-soft/40 px-4 py-3 text-sm text-ink">
      <span className="mt-0.5 h-2 w-2 shrink-0 rounded-full bg-lens" />
      <p>
        <span className="font-semibold">Showing demo data.</span> Couldn't reach the API at{" "}
        <code className="rounded bg-white/60 px-1 py-0.5 font-mono text-xs">{API_BASE_URL}</code>. Start the
        SourceLens backend and set <code className="rounded bg-white/60 px-1 py-0.5 font-mono text-xs">VITE_API_BASE_URL</code>{" "}
        in <code className="rounded bg-white/60 px-1 py-0.5 font-mono text-xs">.env</code> to connect. Changes made here are local
        only until then.
      </p>
    </div>
  );
}
