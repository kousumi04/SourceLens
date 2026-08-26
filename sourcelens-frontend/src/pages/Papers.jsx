import { useRef, useState } from "react";
import Topbar from "../components/Topbar";
import DataTable from "../components/DataTable";
import Modal from "../components/Modal";
import DemoBanner from "../components/DemoBanner";
import { useResource } from "../hooks/useResource";
import { mockPapers } from "../api/mockData";

const API_BASE = "http://localhost:5181/api";

export default function Papers() {
  const papers = useResource("Papers", mockPapers);

  const fileInputRef = useRef(null);

  const [modalOpen, setModalOpen] = useState(false);
  const [selectedPaper, setSelectedPaper] = useState(null);

  const [uploading, setUploading] = useState(false);
  const [uploadError, setUploadError] = useState("");
  const [uploadSuccess, setUploadSuccess] = useState("");

  const openPaper = (paper) => {
    setSelectedPaper(paper);
    setModalOpen(true);
  };

  const handleUploadClick = () => {
    setUploadError("");
    setUploadSuccess("");

    if (fileInputRef.current) {
      fileInputRef.current.click();
    }
  };

  const handleFileChange = async (event) => {
    const file = event.target.files?.[0];

    // Reset input so the same file can be selected again
    event.target.value = "";

    if (!file) {
      return;
    }

    if (!file.name.toLowerCase().endsWith(".pdf")) {
      setUploadError("Please select a PDF file.");
      return;
    }

    setUploading(true);
    setUploadError("");
    setUploadSuccess("");

    try {
      const formData = new FormData();

      formData.append("file", file);

      // Use an existing user ID from your database.
      formData.append("userId", "1");

      const response = await fetch(`${API_BASE}/Papers/upload`, {
        method: "POST",
        body: formData,
      });

      const contentType = response.headers.get("content-type");

      let data;

      if (contentType && contentType.includes("application/json")) {
        data = await response.json();
      } else {
        data = await response.text();
      }

      if (!response.ok) {
        let errorMessage = "Paper upload failed.";

        if (typeof data === "string" && data) {
          errorMessage = data;
        } else if (data?.error) {
          errorMessage = data.error;
        } else if (data?.message) {
          errorMessage = data.message;
        }

        throw new Error(errorMessage);
      }

      setUploadSuccess(
        `Paper uploaded successfully. ${data.claimsExtracted ?? 0} claims extracted.`
      );

      // Refresh the page so the newly uploaded paper appears
      // in the Papers table.
      setTimeout(() => {
        window.location.reload();
      }, 1000);
    } catch (error) {
      console.error("Upload error:", error);

      setUploadError(
        error.message || "Unable to upload the paper."
      );
    } finally {
      setUploading(false);
    }
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
          onClick={handleUploadClick}
          disabled={uploading}
          className="rounded-md bg-lens px-4 py-2 text-sm font-medium text-white transition hover:bg-lens-deep disabled:cursor-not-allowed disabled:opacity-60"
        >
          {uploading ? "Uploading..." : "+ Upload paper"}
        </button>

        <input
          ref={fileInputRef}
          type="file"
          accept=".pdf,application/pdf"
          onChange={handleFileChange}
          className="hidden"
        />
      </Topbar>

      {papers.isDemo && <DemoBanner />}

      {uploadError && (
        <div className="mb-5 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          <strong>Upload failed:</strong> {uploadError}
        </div>
      )}

      {uploadSuccess && (
        <div className="mb-5 rounded-md border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
          {uploadSuccess}
        </div>
      )}

      <DataTable
        columns={columns}
        rows={papers.data}
        onEdit={openPaper}
        onDelete={(row) =>
          papers.remove(row.paperId ?? row.id)
        }
        onAddNew={handleUploadClick}
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
                  ? new Date(
                      selectedPaper.uploadDate
                    ).toLocaleString()
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