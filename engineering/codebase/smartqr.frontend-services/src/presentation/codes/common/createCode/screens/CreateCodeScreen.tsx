import { useEffect, useState } from "react";
import { ButtonType, ColorTone, SizePreset } from "@wow-two-beta/ui/foundation/utils";
import {
  Button,
  ButtonVariant,
  ToggleButton,
  ToggleButtonGroup,
  ToggleButtonGroupVariant,
  ToggleMode,
} from "@wow-two-beta/ui/presentation/actions";
import { Card, Heading, HeadingSize, Text } from "@wow-two-beta/ui/presentation/display";
import { Alert, Spinner } from "@wow-two-beta/ui/presentation/feedback";
import { Center, Grid, Stack } from "@wow-two-beta/ui/presentation/layout";
import { ArrowLeft } from "lucide-react";
import { ContentMode, type CodeDto } from "@/domain/codes";
import {
  CreateCodeSchema,
  emptyCodeCreateUpdateApiRequest,
  oppositeMode,
  toCodeCreateUpdateApiRequest,
  toCopyCodeCreateUpdateApiRequest,
  toCreateCodeRequest,
} from "@/application/codes";
import { codesApiClient, type CodeCreateUpdateApiRequest } from "@/integration/codes";
import { useAppForm } from "@/form";
import { ContentModeDisplays } from "@/presentation/codes/content/components/ContentModeDisplays";
import { ContentView, DesignView, PreviewView } from "../views";

/** Defines the code builder's grouped sections — Content · Design. Rules live under Content: a rule carries content. */
const CodeTab = {
  /** Refers to the identity + content section (name, type, mode, the rules carrying the content). */
  Content: "content",
  /** Refers to the styling section (colors / shape / center). */
  Design: "design",
} as const;

type CodeTab = (typeof CodeTab)[keyof typeof CodeTab];

/** Defines props for the code builder screen. */
export interface CreateCodeScreenProps {
  /** The id of the code to edit (PUT); unset → create (POST). */
  readonly codeId?: string;

  /** The id of a code to copy into a fresh builder — stays create (POST), prefilled from that code (CM5). */
  readonly copyFromId?: string;

  /** The mode the copy is created in; defaults to the opposite of the source code's. */
  readonly copyMode?: ContentMode;

  /** Fires when the user returns to the codes list. */
  readonly onBack?: () => void;

  /** Fires when a save succeeds so the parent refreshes the list. */
  readonly onSaved?: () => void;
}

/** Renders the code builder — create, or edit when `codeId` set. Edit submits a full replace; slug is read-only (printed, immutable). */
export function CreateCodeScreen({ codeId, copyFromId, copyMode, onBack, onSaved }: CreateCodeScreenProps) {
  const isEdit = Boolean(codeId);
  // A copy loads the source code the same way an edit does, then submits as a create.
  const sourceId = codeId ?? copyFromId;

  // The active builder section.
  const [tab, setTab] = useState<CodeTab>(CodeTab.Content);
  // The loaded / just-saved server record — backs the read-only short link + the preview value. Not form data.
  const [existingCode, setExistingCode] = useState<CodeDto | null>(null);
  // The saved code after a successful create/update — drives the post-save panel. Not form data.
  const [saved, setSaved] = useState<CodeDto | null>(null);
  // The mode a copy is being created in — set once the source loads, so the header can name it.
  const [copiedInto, setCopiedInto] = useState<ContentMode | null>(null);
  // Edit / copy fetch state.
  const [loading, setLoading] = useState(Boolean(sourceId));
  const [loadError, setLoadError] = useState<string | null>(null);

  const form = useAppForm<CodeCreateUpdateApiRequest>({
    defaultValues: emptyCodeCreateUpdateApiRequest(),
    schema: CreateCodeSchema,
    // onSubmit is the only failure path — a thrown SDK ApiError maps to fields, the remainder to submitError.
    onSubmit: async (values) => {
      const request = toCreateCodeRequest(values);
      const dto = codeId ? await codesApiClient.update(codeId, request) : await codesApiClient.create(request);
      setSaved(dto);
      setExistingCode(dto);
      onSaved?.();
    },
  });

  // Edit / copy: load once, then reseed the form values + dirty baseline via reset(data). A copy never sets
  // `existingCode` — that backs the source's short link and preview mode, and the copy is a different symbol.
  useEffect(() => {
    if (!sourceId) return;
    let cancelled = false;
    setLoading(true);
    setLoadError(null);
    codesApiClient
      .get(sourceId)
      .then((code) => {
        if (cancelled) return;
        if (codeId) {
          setExistingCode(code);
          form.reset(toCodeCreateUpdateApiRequest(code));
          return;
        }
        const mode = copyMode ?? oppositeMode(code.mode);
        setCopiedInto(mode);
        form.reset(toCopyCodeCreateUpdateApiRequest(code, mode));
      })
      .catch((e: unknown) => {
        if (!cancelled) setLoadError(e instanceof Error ? e.message : "Failed to load the code");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [sourceId, codeId, copyMode, form]);

  // "Create another" clears only the saved panel — the builder keeps its values for a quick variant.
  function handleCreateAnother() {
    setSaved(null);
  }

  if (loading) {
    return (
      <Center className="min-h-[60vh]">
        <Spinner size={SizePreset.Lg} label="Loading code" />
      </Center>
    );
  }

  return (
    <Stack gap="6">
      <Stack gap="2">
        {onBack && (
          <Button
            variant={ButtonVariant.Ghost}
            tone={ColorTone.Neutral}
            size={SizePreset.Sm}
            leadingSlot={<ArrowLeft size={16} />}
            className="-ml-2 self-start"
            onClick={onBack}
          >
            Back to codes
          </Button>
        )}
        <div>
          <Heading level={1} size={HeadingSize.Xl} weight="bold">
            {isEdit ? "Edit code" : copiedInto ? "Copy code" : "Create a code"}
          </Heading>
          <Text color="muted">
            {isEdit
              ? "Update the destination and routing — the printed code keeps working."
              : copiedInto
                ? `A new ${ContentModeDisplays[copiedInto].label.toLowerCase()} code with the same content — the original keeps working.`
                : "One code, many destinations — and it never expires."}
          </Text>
        </div>
      </Stack>

      <Grid columns={{ base: "1", lg: "2" }} gap="6">
        {/* ── Builder ── (className="contents" so the form wrapper doesn't disturb the grid layout) */}
        <form onSubmit={form.handleSubmit} className="contents">
          <Card className="surface-soft flex flex-col gap-5 p-6">
            <ToggleButtonGroup
              variant={ToggleButtonGroupVariant.Segmented}
              type={ToggleMode.Single}
              value={tab}
              onValueChange={(v) => v && setTab(v as CodeTab)}
              aria-label="Builder section"
            >
              <ToggleButton value={CodeTab.Content} className="flex-1">Content</ToggleButton>
              <ToggleButton value={CodeTab.Design} className="flex-1">Design</ToggleButton>
            </ToggleButtonGroup>

            {/* key={tab} re-mounts on switch so the fade-through re-fires; motion-safe respects reduced-motion. */}
            <div key={tab} className="flex flex-col gap-5 motion-safe:animate-fade-in">
              {tab === CodeTab.Content && (
                <ContentView form={form} isEdit={isEdit} existingCode={existingCode ?? undefined} />
              )}

              {tab === CodeTab.Design && <DesignView form={form} />}
            </div>

            <form.Subscribe selector={(s) => s.isSubmitting}>
              {(busy) => (
                <Button
                  type={ButtonType.Submit}
                  tone={ColorTone.Primary}
                  isFullWidth
                  isLoading={busy}
                  loadingText={isEdit ? "Saving…" : "Creating…"}
                >
                  {isEdit ? "Save changes" : "Create code"}
                </Button>
              )}
            </form.Subscribe>

            {loadError && <Alert severity="danger" description={loadError} />}
            <form.Subscribe selector={(s) => s.submitError}>
              {(submitError) => submitError && <Alert severity="danger" description={submitError.message} />}
            </form.Subscribe>
          </Card>
        </form>

        {/* ── Preview ── driven by stable value slices (style / content / barcodeFormat); re-renders only when
            a design/content/format change lands, never on name or rule-row keystrokes. */}
        <form.Subscribe selector={(s) => s.values.style}>
          {(style) => (
            <form.Subscribe selector={(s) => s.values.rules}>
              {(rules) => (
                <form.Subscribe selector={(s) => s.values.barcodeFormat}>
                  {(barcodeFormat) => (
                    <form.Subscribe selector={(s) => s.values.mode}>
                      {(mode) => (
                      <PreviewView
                        previewMode={existingCode?.mode ?? mode ?? ContentMode.Static}
                        previewRules={rules}
                        previewBarcodeFormat={barcodeFormat}
                        previewStyle={style}
                        foreground={style.foregroundColor}
                        background={style.backgroundColor}
                        transparentBackground={style.transparentBackground}
                        gradient={style.gradient ?? null}
                        saved={saved}
                        isEdit={isEdit}
                        onBack={onBack}
                        onCreateAnother={handleCreateAnother}
                      />
                      )}
                    </form.Subscribe>
                  )}
                </form.Subscribe>
              )}
            </form.Subscribe>
          )}
        </form.Subscribe>
      </Grid>
    </Stack>
  );
}
