import { useState } from "react";
import Topbar from "../components/Topbar";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";
import DemoBanner from "../components/DemoBanner";
import { useResource } from "../hooks/useResource";
import { mockPapers } from "../api/mockData";

export default function Papers() {
  const papers = useResource("Papers", mockPapers);

  const [modalOpen, setModalOpen] = useState(false);
  const [selectedPaper, setSelectedPaper] = useState(null);

  const openPaper = (paper) => {
    setSelectedPaper(paper);
    setModalOpen(true);
  };

  const columns = [
    {
      key: "title",
      label: "Paper",
      render: (r) => (
        <div>
          <p className="font-display text-[15px] leading-snug text-ink">
            {r.title || "Untitled paper"}
          </p>

          <p className="mt-1 text-xs text-ink-muted">
            {r.fileName || "No file name"}
          </p>
        </div>
      ),
    },

    {
      key: "status",
      label: "Status",
      render: (r) => (
        <span
          className={`rounded-full px-2.5 py-1 text-xs font-semibold ${
            r.status === "Processed"
              ? "text-support bg-support-soft"
              : "text-pending bg-pending-soft"
          }`}
        >
          {r.status || "Unknown"}
        </span>
      ),
    },

    {
      key: "claims",
      label: "Claims",
      render: (r) => (
        <span className="font-mono text-sm">
          {Array.isArray(r.claims) ? r.claims.length : 0}
        </span>
      ),
    },

    {
      key: "uploadDate",
      label: "Uploaded",
      render: (r) => (
        <span className="font-mono text-sm text-ink-muted">
          {r.uploadDate
            ? new Date(r.uploadDate).toLocaleDateString()
            : "—"}
        </span>
      ),
    },

    {
      key: "paperId",
      label: "ID",
      render: (r) => (
        <span className="font-mono text-sm text-ink-muted">
          #{r.paperId ?? r.id}
        </span>
      ),
    },
  ];

  return (
    <div className="animate-fade-up">
      <Topbar eyebrow="Records" title="Research Papers">
        <button
          onClick={() => alert("Upload papers using the backend upload API for now.")}
          className="rounded-md bg-lens px-4 py-2 text-sm font-medium text-white transition hover:bg-lens-deep"
        >
          + Upload paper
        </button>
      </Topbar>

      {papers.isDemo && <DemoBanner />}

      <DataTable
        columns={columns}
        rows={papers.data}
        onEdit={openPaper}
        onDelete={(row) =>
          papers.remove(row.paperId ?? row.id)
        }
        onAddNew={() => alert("Upload papers using the backend upload API for now.")}
        emptyTitle="No papers uploaded yet"
        emptyBody="Upload a research paper to begin extracting claims from it."
      />

      <Modal
        open={modalOpen}
        title={selectedPaper?.title || "Paper details"}
        onClose={() => {
          setModalOpen(false);
          setSelectedPaper(null);
        }}
      >
        {selectedPaper && (
          <div className="space-y-4">
            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-ink-muted">
                Paper ID
              </p>
              <p className="mt-1 font-mono text-sm">
                #{selectedPaper.paperId ?? selectedPaper.id}
              </p>
            </div>

            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-ink-muted">
                File
              </p>
              <p className="mt-1 text-sm text-ink">
                {selectedPaper.fileName || "—"}
              </p>
            </div>

            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-ink-muted">
                Status
              </p>
              <p className="mt-1 text-sm text-ink">
                {selectedPaper.status || "—"}
              </p>
            </div>

            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-ink-muted">
                Uploaded
              </p>
              <p className="mt-1 text-sm text-ink">
                {selectedPaper.uploadDate
                  ? new Date(selectedPaper.uploadDate).toLocaleString()
                  : "—"}
              </p>
            </div>

            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-ink-muted">
                Extracted claims
              </p>

              {Array.isArray(selectedPaper.claims) &&
              selectedPaper.claims.length > 0 ? (
                <div className="mt-2 space-y-2">
                  {selectedPaper.claims.map((claim) => (
                    <div
                      key={claim.claimId}
                      className="rounded-md border border-line bg-paper p-3"
                    >
                      <p className="text-sm text-ink">
                        {claim.claimText}
                      </p>

                      <p className="mt-1 text-xs text-ink-muted">
                        Page {claim.pageNumber}
                      </p>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="mt-2 text-sm text-ink-muted">
                  No claims available.
                </p>
              )}
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}