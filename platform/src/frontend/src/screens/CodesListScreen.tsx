import { useEffect, useState } from "react";
import { Button, CopyButton } from "@wow-two-beta/ui/actions";
import { SearchInput } from "@wow-two-beta/ui/forms";
import { Badge, Card, EmptyState, Heading } from "@wow-two-beta/ui/display";
import { Spinner } from "@wow-two-beta/ui/feedback";
import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@wow-two-beta/ui/overlays";
import { Pencil, Plus, QrCode, Trash2 } from "lucide-react";
import { deleteCode, listCodes, setCodeActive } from "../api";
import type { CodeDto } from "../types";

interface CodesListScreenProps {
  /** Open the empty builder to create a new code. */
  onCreate: () => void;
  /** Open the builder in edit mode for an existing code. */
  onEdit: (id: string) => void;
}

/**
 * Owner's codes dashboard: searchable list with per-row Edit / Enable-Disable / Delete actions.
 * Every fetch + mutation is owner-scoped via the credentials cookie carried by the api client.
 */
export function CodesListScreen({ onCreate, onEdit }: CodesListScreenProps) {
  const [codes, setCodes] = useState<CodeDto[]>([]);
  const [query, setQuery] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [pendingDelete, setPendingDelete] = useState<CodeDto | null>(null);

  async function load(q?: string) {
    setLoading(true);
    setError(null);
    try {
      setCodes(await listCodes(q));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Something went wrong");
    } finally {
      setLoading(false);
    }
  }

  // Debounce the search box so each keystroke doesn't fire a request.
  useEffect(() => {
    const handle = setTimeout(() => load(query), 250);
    return () => clearTimeout(handle);
  }, [query]);

  async function toggleActive(code: CodeDto) {
    setBusyId(code.id);
    setError(null);
    try {
      const updated = await setCodeActive(code.id, !code.isActive);
      setCodes((prev) => prev.map((c) => (c.id === updated.id ? updated : c)));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Something went wrong");
    } finally {
      setBusyId(null);
    }
  }

  async function confirmDelete() {
    if (!pendingDelete) return;
    const target = pendingDelete;
    setBusyId(target.id);
    setError(null);
    try {
      await deleteCode(target.id);
      setCodes((prev) => prev.filter((c) => c.id !== target.id));
      setPendingDelete(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Something went wrong");
    } finally {
      setBusyId(null);
    }
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <Heading level={1} className="text-2xl font-bold">
            Your codes
          </Heading>
          <p className="text-muted-foreground">Manage destinations and routing — nothing ever expires.</p>
        </div>
        <Button tone="primary" leadingSlot={<Plus size={16} />} onClick={onCreate}>
          Create new
        </Button>
      </div>

      <div className="max-w-md">
        <SearchInput
          value={query}
          placeholder="Search by name or destination…"
          onChange={(e) => setQuery(e.target.value)}
          onClear={() => setQuery("")}
        />
      </div>

      {error && (
        <p className="rounded-md border border-destructive/40 bg-destructive/5 p-3 text-sm text-destructive">
          {error}
        </p>
      )}

      {loading ? (
        <div className="flex min-h-[40vh] items-center justify-center">
          <Spinner size="lg" label="Loading codes" />
        </div>
      ) : codes.length === 0 ? (
        <Card className="p-6">
          <EmptyState
            icon={<QrCode size={32} />}
            title={query ? "No matching codes" : "No codes yet"}
            description={
              query
                ? "Try a different search term."
                : "Create your first programmable code — one code, many destinations."
            }
            actions={
              !query && (
                <Button tone="primary" leadingSlot={<Plus size={16} />} onClick={onCreate}>
                  Create new
                </Button>
              )
            }
          />
        </Card>
      ) : (
        <div className="flex flex-col gap-3">
          {codes.map((code) => (
            <Card key={code.id} className="flex flex-col gap-4 p-4 sm:flex-row sm:items-center">
              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-2">
                  <span className="truncate font-medium" title={code.name}>
                    {code.name}
                  </span>
                  <Badge variant={code.isActive ? "success" : "neutral"} size="sm">
                    {code.isActive ? "Active" : "Inactive"}
                  </Badge>
                </div>
                <div className="mt-1 flex items-center gap-2">
                  <a
                    href={code.shortUrl}
                    target="_blank"
                    rel="noreferrer"
                    className="truncate text-sm text-muted-foreground hover:text-foreground hover:underline"
                    title={code.shortUrl}
                  >
                    {code.shortUrl}
                  </a>
                  <CopyButton size="xs" variant="ghost" tone="neutral" text={code.shortUrl} aria-label="Copy short URL" />
                </div>
                <p className="mt-1 truncate text-sm text-muted-foreground" title={code.fallbackUrl}>
                  → {code.fallbackUrl}
                </p>
              </div>

              <div className="flex shrink-0 items-center gap-4 sm:gap-6">
                <div className="text-right">
                  <div className="text-lg font-semibold tabular-nums">{code.scanCount.toLocaleString()}</div>
                  <div className="text-xs text-muted-foreground">scans</div>
                </div>

                <div className="flex items-center gap-1">
                  <Button
                    size="sm"
                    variant="ghost"
                    tone="neutral"
                    leadingSlot={<Pencil size={15} />}
                    isDisabled={busyId === code.id}
                    onClick={() => onEdit(code.id)}
                  >
                    Edit
                  </Button>
                  <Button
                    size="sm"
                    variant="soft"
                    tone="neutral"
                    isLoading={busyId === code.id}
                    onClick={() => toggleActive(code)}
                  >
                    {code.isActive ? "Disable" : "Enable"}
                  </Button>
                  <Button
                    size="sm"
                    variant="ghost"
                    tone="danger"
                    shape="square"
                    aria-label="Delete code"
                    isDisabled={busyId === code.id}
                    onClick={() => setPendingDelete(code)}
                  >
                    <Trash2 size={16} />
                  </Button>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}

      <AlertDialog
        open={pendingDelete !== null}
        onOpenChange={(open) => {
          if (!open) setPendingDelete(null);
        }}
      >
        <AlertDialogContent>
          <DialogHeader>
            <DialogTitle>Delete this code?</DialogTitle>
            <DialogDescription>
              “{pendingDelete?.name}” and its routing rules will be permanently deleted. The printed code
              will stop resolving. This can’t be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <Button
              tone="danger"
              isLoading={busyId === pendingDelete?.id}
              loadingText="Deleting…"
              onClick={confirmDelete}
            >
              Delete
            </Button>
          </DialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
