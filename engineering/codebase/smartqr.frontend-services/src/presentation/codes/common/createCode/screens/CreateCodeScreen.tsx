import { useEffect, useMemo, useState } from "react";
import { ColorTone, SizePreset } from "@wow-two-beta/ui/foundation/utils";
import { Button, ButtonVariant, ToggleButton, ToggleButtonGroup, ToggleButtonGroupVariant, ToggleMode } from "@wow-two-beta/ui/presentation/actions";
import { Card, Heading, HeadingSize, Text } from "@wow-two-beta/ui/presentation/display";
import { Alert, Spinner } from "@wow-two-beta/ui/presentation/feedback";
import { Center, Grid, Stack } from "@wow-two-beta/ui/presentation/layout";
import { ArrowLeft } from "lucide-react";
import {
  BarcodeFormat,
  CodeType,
  EccLevel,
  FinderShape,
  GradientType,
  ModuleShape,
  type CodeDto,
  type Gradient,
  type DesignEmojiOverlay,
  type PreviewStyle,
  type RuleDraft,
} from "@/domain/codes";
import { ContentTypeId, isDynamicType, type CodeContent } from "@/domain/codes/content";
import { REDIRECT_BASE } from "@/integration/common";
import { createCode, getCode, updateCode } from "@/integration/codes";
import { ContentView, DesignView, RoutingView, PreviewView } from "../views";

/** Defines the code builder's grouped sections (Layout D — Content · Design · Routing). */
const CodeTab = {
  /** Refers to the content-type + payload section. */
  Content: "content",
  /** Refers to the styling section (colors / shape / center). */
  Design: "design",
  /** Refers to the routing-rules section. */
  Routing: "routing",
} as const;

type CodeTab = (typeof CodeTab)[keyof typeof CodeTab];

/** Maps a code's persisted rules to the builder's draft shape (adds client-side keys). */
function toDrafts(code: CodeDto): RuleDraft[] {
  return code.rules.map((r) => ({
    id: crypto.randomUUID(),
    order: r.order,
    conditionType: r.conditionType,
    conditionValue: r.conditionValue ?? "",
    destination: r.destination,
  }));
}

/** Maps a persisted gradient (wire shape) back to the builder's gradient, or null for a solid foreground. */
function gradientFromDto(g: CodeDto["style"]["gradient"]): Gradient | null {
  if (!g) return null;
  const stops = g.stops.map((s) => ({ color: s.color, offset: s.offset }));
  return g.type === GradientType.Linear
    ? { type: GradientType.Linear, angle: g.angle, stops }
    : { type: GradientType.Radial, radius: g.radius, stops };
}

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

  const [name, setName] = useState("");
  const [symbology, setSymbology] = useState<BarcodeFormat>(BarcodeFormat.QrCode);
  const [foreground, setForeground] = useState("#18181b");
  const [background, setBackground] = useState("#ffffff");
  // Code-styling shapes (v0.5) — default `square` so the render is unchanged until picked.
  // Layout D (v0.6): 3 grouped tabs — Content · Design (accordion) · Routing.
  const [tab, setTab] = useState<CodeTab>(CodeTab.Content);
  const [moduleShape, setModuleShape] = useState<ModuleShape>(ModuleShape.Square);
  const [finderShape, setFinderShape] = useState<FinderShape>(FinderShape.Square);
  const [finderDotShape, setFinderDotShape] = useState<FinderShape>(FinderShape.Square);
  const [gradient, setGradient] = useState<Gradient | null>(null);
  const [transparentBackground, setTransparentBackground] = useState(false);
  const [emoji, setEmoji] = useState<DesignEmojiOverlay | null>(null);
  const [rules, setRules] = useState<RuleDraft[]>([]);
  const [existingCode, setExistingCode] = useState<CodeDto | null>(null);
  // The typed content the code carries — the builder holds it directly (it *is* the wire shape). Defaults to a
  // `url` forwarder seeded with a sample so the preview renders on first load.
  const [content, setContent] = useState<CodeContent>({ type: "url", url: "https://example.com" });

  const [loading, setLoading] = useState(isEdit);
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState<CodeDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Edit mode: load once, prefill fields.
  useEffect(() => {
    if (!codeId) return;
    let cancelled = false;
    setLoading(true);
    setError(null);
    getCode(codeId)
      .then((code) => {
        if (cancelled) return;
        setExistingCode(code);
        setName(code.name);
        setSymbology(code.barcodeFormat);
        setRules(toDrafts(code));
        setForeground(code.style.foregroundColor);
        setBackground(code.style.backgroundColor);
        setModuleShape(code.style.moduleShape);
        setFinderShape(code.style.finderShape);
        setFinderDotShape(code.style.finderDotShape);
        setGradient(gradientFromDto(code.style.gradient));
        setTransparentBackground(code.style.transparentBackground);
        setEmoji(code.style.emoji ?? null);
        // Load the typed content directly. A legacy code (no typed content) opens as an editable `url` forwarder
        // seeded from its stored fallback.
        setContent(code.content ?? { type: "url", url: code.fallbackUrl });
      })
      .catch((e: unknown) => {
        if (!cancelled) setError(e instanceof Error ? e.message : "Failed to load the code");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [codeId]);

  // The preview endpoint encodes the typed content server-side (same encoder as the saved asset → true parity).
  // Static content bakes its payload; dynamic (url / mobileApp) falls back to the short link / sample URL.
  const previewContent = content;
  const urlDestination = content.type === ContentTypeId.Url ? content.url : "";
  const previewValue = saved?.shortUrl ?? existingCode?.shortUrl ?? (urlDestination || `${REDIRECT_BASE}/preview`);

  // The preview endpoint's coarse kind: QR symbology → "qr", any other (1D/2D) → "barcode".
  const previewCodeType: CodeType = symbology === BarcodeFormat.QrCode ? CodeType.Qr : CodeType.Barcode;

  // Defaults (ECC / quiet-zone / logo) aren't surfaced in the builder yet — send the renderer's
  // standard defaults. Surface them as inputs in a later iteration. Shapes default to `square`,
  // so the render is unchanged until the user picks a style.
  const previewStyle: PreviewStyle = useMemo(
    () => ({
      foregroundColor: foreground,
      backgroundColor: background,
      transparentBackground,
      eccLevel: EccLevel.Q,
      quietZoneModules: 2,
      logo: null,
      moduleShape,
      finderShape,
      finderDotShape,
      gradient,
      emoji,
    }),
    [foreground, background, transparentBackground, moduleShape, finderShape, finderDotShape, gradient, emoji],
  );

  async function handleSubmit() {
    setError(null);
    // Client-side guard (the backend still validates): a mobile app link needs at least one destination.
    if (content.type === ContentTypeId.MobileApp && !(content.appStore || content.playStore || content.other)) {
      setError("Add at least one link — App Store, Google Play, or a custom URL for other devices.");
      return;
    }
    setSaving(true);
    // Content shapes the request: static bakes its payload server-side (no redirect, no rules); a self-routed
    // type (mobileApp) is derived server-side from its fields; a plain URL keeps its own routing rules.
    const isStatic = !isDynamicType(content.type);
    const selfRouted = content.type === ContentTypeId.MobileApp; // backend derives fallback + device rules from the fields
    const request = {
      name: name.trim() || "Untitled code",
      codeType: CodeType.Qr,
      barcodeFormat: symbology,
      // Static bakes a payload; mobileApp's fallback + rules are derived on the server — both send empty here.
      // A plain URL forwards to its own destination.
      fallbackUrl: isStatic || selfRouted ? "" : content.type === ContentTypeId.Url ? content.url.trim() : "",
      rules:
        isStatic || selfRouted
          ? []
          : rules.map((r) => ({
              order: r.order,
              conditionType: r.conditionType,
              conditionValue: r.conditionValue.trim(),
              destination: r.destination.trim(),
            })),
      style: previewStyle,
      content,
    };
    try {
      const dto = codeId ? await updateCode(codeId, request) : await createCode(request);
      setSaved(dto);
      setExistingCode(dto);
      onSaved?.();
    } catch (e) {
      setError(e instanceof Error ? e.message : "Something went wrong");
    } finally {
      setSaving(false);
    }
  }

  function reset() {
    setSaved(null);
    setError(null);
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
        {/* ── Builder ── */}
        <Card className="surface-soft flex flex-col gap-5 p-6">
          <ToggleButtonGroup variant={ToggleButtonGroupVariant.Segmented}
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
              <ContentView
                isEdit={isEdit}
                existingCode={existingCode}
                name={name}
                onNameChange={setName}
                content={content}
                onContentChange={setContent}
              />
            )}

            {tab === CodeTab.Design && (
              <DesignView
                symbology={symbology}
                onSymbologyChange={setSymbology}
                foreground={foreground}
                onForegroundChange={setForeground}
                gradient={gradient}
                onGradientChange={setGradient}
                background={background}
                onBackgroundChange={setBackground}
                transparentBackground={transparentBackground}
                onTransparentBackgroundChange={setTransparentBackground}
                moduleShape={moduleShape}
                onModuleShapeChange={setModuleShape}
                finderShape={finderShape}
                onFinderShapeChange={setFinderShape}
                finderDotShape={finderDotShape}
                onFinderDotShapeChange={setFinderDotShape}
                emoji={emoji}
                onEmojiChange={setEmoji}
              />
            )}

            {tab === CodeTab.Routing && <RoutingView rules={rules} onRulesChange={setRules} />}
          </div>

          <Button
            tone={ColorTone.Primary}
            isFullWidth
            isLoading={saving}
            loadingText={isEdit ? "Saving…" : "Creating…"}
            onClick={handleSubmit}
          >
            {isEdit ? "Save changes" : "Create code"}
          </Button>

          {error && <Alert severity="danger" description={error} />}
        </Card>

        {/* ── Preview ── */}
        <PreviewView
          previewValue={previewValue}
          previewContent={previewContent}
          previewCodeType={previewCodeType}
          previewStyle={previewStyle}
          foreground={foreground}
          background={background}
          transparentBackground={transparentBackground}
          gradient={gradient}
          saved={saved}
          isEdit={isEdit}
          onBack={onBack}
          onCreateAnother={reset}
        />
      </Grid>
    </Stack>
  );
}
