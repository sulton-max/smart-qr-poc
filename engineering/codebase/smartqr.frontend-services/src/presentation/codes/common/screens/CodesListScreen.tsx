import { useEffect, useState } from "react";
import { ColorTone, SizePreset } from "@wow-two-beta/ui/foundation/utils";
import { Button, ButtonVariant, CopyButton } from "@wow-two-beta/ui/presentation/actions";
import { SearchInput } from "@wow-two-beta/ui/presentation/forms";
import { Card, EmptyState, Heading, HeadingSize, Text } from "@wow-two-beta/ui/presentation/display";
import { Alert, Spinner } from "@wow-two-beta/ui/presentation/feedback";
import { Center, HStack, Stack } from "@wow-two-beta/ui/presentation/layout";
import {
  AlertModal,
  AlertModalCancel,
  AlertModalContent,
  ModalDescription,
  ModalFooter,
  ModalHeader,
  ModalTitle,
} from "@wow-two-beta/ui/presentation/overlays";
import { Pencil, Plus, QrCode, Trash2 } from "lucide-react";
import type { CodeDto } from "@/domain/codes/common";
import { deleteCode, listCodes, setCodeActive } from "@/integration/codes";

/** Defines props for the codes dashboard screen. */
interface CodesListScreenProps {
  /** Fires when the user opens the empty builder. */
  readonly onCreate: () => void;

  /** Fires when the user opens the builder in edit mode. */
  readonly onEdit: (id: string) => void;
}

/** Renders the codes dashboard — searchable list, per-row Edit / Enable-Disable / Delete. Fetches and mutations are owner-scoped via the credentials cookie. */
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

  // Debounce search — one request per pause, not per keystroke.
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
    <Stack gap="6">
      <HStack wrap="wrap" align="end" justify="between" gap="4">
        <div>
          <Heading level={1} size={HeadingSize.Xl} weight="bold">
            Your codes
          </Heading>
          <Text color="muted">Manage destinations and routing — nothing ever expires.</Text>
        </div>
        <Button tone={ColorTone.Primary} leadingSlot={<Plus size={16} />} onClick={onCreate}>
          Create new
        </Button>
      </HStack>

      <div className="max-w-md">
        <SearchInput
          value={query}
          placeholder="Search by name or destination…"
          aria-label="Search codes"
          onChange={(e) => setQuery(e.target.value)}
          onClear={() => setQuery("")}
        />
      </div>

      {error && <Alert severity="danger" description={error} />}

      {loading ? (
        <Center className="min-h-[40vh]">
          <Spinner size={SizePreset.Lg} label="Loading codes" />
        </Center>
      ) : codes.length === 0 ? (
        <Card className="surface-soft p-6">
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
                <Button tone={ColorTone.Primary} leadingSlot={<Plus size={16} />} onClick={onCreate}>
                  Create new
                </Button>
              )
            }
          />
        </Card>
      ) : (
        <Stack gap="3">
          {codes.map((code) => (
            <Card key={code.id} className="surface-soft flex flex-col gap-4 p-4 sm:flex-row sm:items-center">
              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-2">
                  <span className="truncate font-medium" title={code.name}>
                    {code.name}
                  </span>
                  <span className="inline-flex shrink-0 items-center gap-1.5 rounded-full bg-muted px-2 py-0.5 text-xs font-medium text-muted-foreground">
                    <span className={`size-1.5 rounded-full ${code.isActive ? "bg-accent" : "bg-subtle-foreground"}`} />
                    {code.isActive ? "Active" : "Inactive"}
                  </span>
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
                  <CopyButton size={SizePreset.Xs} variant={ButtonVariant.Ghost} tone={ColorTone.Neutral} text={code.shortUrl} aria-label="Copy short URL" />
                </div>
                <Text size={SizePreset.Sm} color="muted" isTruncated className="mt-1" title={code.fallbackUrl}>
                  → {code.fallbackUrl}
                </Text>
              </div>

              <div className="flex shrink-0 items-center gap-4 sm:gap-6">
                <div className="text-right">
                  <div className="text-lg font-semibold tabular-nums">{code.scanCount.toLocaleString()}</div>
                  <Text size={SizePreset.Xs} color="muted">scans</Text>
                </div>

                <div className="flex items-center gap-1">
                  <Button
                    size={SizePreset.Sm}
                    variant={ButtonVariant.Ghost}
                    tone={ColorTone.Neutral}
                    leadingSlot={<Pencil size={15} />}
                    isDisabled={busyId === code.id}
                    onClick={() => onEdit(code.id)}
                  >
                    Edit
                  </Button>
                  <Button
                    size={SizePreset.Sm}
                    variant={ButtonVariant.Soft}
                    tone={ColorTone.Neutral}
                    isLoading={busyId === code.id}
                    onClick={() => toggleActive(code)}
                  >
                    {code.isActive ? "Disable" : "Enable"}
                  </Button>
                  <Button
                    size={SizePreset.Sm}
                    variant={ButtonVariant.Ghost}
                    tone={ColorTone.Danger}
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
        </Stack>
      )}

      <AlertModal
        open={pendingDelete !== null}
        onOpenChange={(open) => {
          if (!open) setPendingDelete(null);
        }}
      >
        <AlertModalContent>
          <ModalHeader>
            <ModalTitle>Delete this code?</ModalTitle>
            <ModalDescription>
              “{pendingDelete?.name}” and its routing rules will be permanently deleted. The printed code
              will stop resolving. This can’t be undone.
            </ModalDescription>
          </ModalHeader>
          <ModalFooter>
            <AlertModalCancel>Cancel</AlertModalCancel>
            <Button
              tone={ColorTone.Danger}
              isLoading={busyId === pendingDelete?.id}
              loadingText="Deleting…"
              onClick={confirmDelete}
            >
              Delete
            </Button>
          </ModalFooter>
        </AlertModalContent>
      </AlertModal>
    </Stack>
  );
}
