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
  toCodeCreateUpdateApiRequest,
  toCreateCodeRequest,
} from "@/application/codes";
import { codesApiClient, type CodeCreateUpdateApiRequest } from "@/integration/codes";
import { useAppForm } from "@/form";
import { ContentView, DesignView, RoutingView, PreviewView } from "../views";

/** Defines the code builder's grouped sections — Content · Design · Routing. */
const CodeTab = {
  /** Refers to the content-type + payload section. */
  Content: "content",
  /** Refers to the styling section (colors / shape / center). */
  Design: "design",
  /** Refers to the routing-rules section. */
  Routing: "routing",
} as const;

type CodeTab = (typeof CodeTab)[keyof typeof CodeTab];

/** Defines props for the code builder screen. */
export interface CreateCodeScreenProps {
  /** The id of the code to edit (PUT); unset → create (POST). */
  readonly codeId?: string;

  /** Fires when the user returns to the codes list. */
  readonly onBack?: () => void;

  /** Fires when a save succeeds so the parent refreshes the list. */
  readonly onSaved?: () => void;
}

/** Renders the code builder — create, or edit when `codeId` set. Edit submits a full replace; slug is read-only (printed, immutable). */
export function CreateCodeScreen({ codeId, onBack, onSaved }: CreateCodeScreenProps) {
  const isEdit = Boolean(codeId);

  // The active builder section.
  const [tab, setTab] = useState<CodeTab>(CodeTab.Content);
  // The loaded / just-saved server record — backs the read-only short link + the preview value. Not form data.
  const [existingCode, setExistingCode] = useState<CodeDto | null>(null);
  // The saved code after a successful create/update — drives the post-save panel. Not form data.
  const [saved, setSaved] = useState<CodeDto | null>(null);
  // Edit-mode fetch state.
  const [loading, setLoading] = useState(isEdit);
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

  // Edit mode: load once, then reseed the form values + dirty baseline via reset(data).
  useEffect(() => {
    if (!codeId) return;
    let cancelled = false;
    setLoading(true);
    setLoadError(null);
    codesApiClient
      .get(codeId)
      .then((code) => {
        if (cancelled) return;
        setExistingCode(code);
        form.reset(toCodeCreateUpdateApiRequest(code));
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
  }, [codeId, form]);

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
            {isEdit ? "Edit code" : "Create a code"}
          </Heading>
          <Text color="muted">
            {isEdit
              ? "Update the destination and routing — the printed code keeps working."
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
              <ToggleButton value={CodeTab.Routing} className="flex-1">Routing</ToggleButton>
            </ToggleButtonGroup>

            {/* key={tab} re-mounts on switch so the fade-through re-fires; motion-safe respects reduced-motion. */}
            <div key={tab} className="flex flex-col gap-5 motion-safe:animate-fade-in">
              {tab === CodeTab.Content && (
                <ContentView form={form} isEdit={isEdit} existingCode={existingCode ?? undefined} />
              )}

              {tab === CodeTab.Design && <DesignView form={form} />}

              {tab === CodeTab.Routing && <RoutingView form={form} />}
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
