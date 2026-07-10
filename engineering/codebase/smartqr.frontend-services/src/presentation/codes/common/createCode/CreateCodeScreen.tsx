import { useEffect, useMemo, useState } from "react";
import { ColorTone, SizePreset, SurfaceVariant } from "@wow-two-beta/ui/foundation/utils";
import { Button, ButtonVariant, CopyButton, ToggleButton, ToggleButtonGroup, ToggleButtonGroupVariant, ToggleMode } from "@wow-two-beta/ui/presentation/actions";
import { Card, Heading, HeadingSize, Text } from "@wow-two-beta/ui/presentation/display";
import { Alert, Spinner } from "@wow-two-beta/ui/presentation/feedback";
import { Center, Grid, Stack, Surface } from "@wow-two-beta/ui/presentation/layout";
import { ArrowLeft } from "lucide-react";
import {
  BarcodeFormat,
  CodeType,
  EccLevel,
  FinderShape,
  GradientType,
  ImageFormat,
  ModuleShape,
  type CodeDto,
  type Gradient,
  type DesignEmojiOverlay,
  type PreviewStyle,
  type RuleDraft,
} from "@/domain/codes";
import { ContentMode, ContentTypeId, contentType, buildContent, contentToValues, isDynamicContent, type FieldValues } from "@/domain/codes/content";
import { REDIRECT_BASE } from "@/integration/common";
import { codeImageUrl, createCode, getCode, updateCode } from "@/integration/codes";
import { ContrastCallout } from "@/presentation/codes/design";
import { QrPreview } from "./QrPreview";
import { ContentView, DesignView, RoutingView } from "./views";

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
  const [fallbackUrl, setFallbackUrl] = useState("https://example.com");
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
  const [existing, setExisting] = useState<CodeDto | null>(null);
  // v0.7 iter1: the chosen content type + its form values. `url` is the dynamic forwarder
  // (its single field binds to `fallbackUrl`); other types drive the preview from their payload.
  const [contentTypeId, setContentTypeId] = useState<ContentTypeId>(ContentTypeId.Url);
  const [contentValues, setContentValues] = useState<FieldValues>({});

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
        setExisting(code);
        setName(code.name);
        setFallbackUrl(code.fallbackUrl);
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
        // Round-trip the content form: normalize the discriminator to a content-type id, then project the typed
        // content back to the builder's flat field values (legacy codes carry no content).
        if (code.content) {
          const typeId = code.content.type;
          setContentTypeId(typeId);
          const values = contentToValues(code.content);
          if (typeId === ContentTypeId.Url) setFallbackUrl(values.url ?? code.fallbackUrl);
          else setContentValues(values);
        }
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
  // Static content bakes its payload; dynamic (url / mobileApp) falls back to the short link.
  const contentDef = contentType(contentTypeId);
  const previewContent = buildContent(contentTypeId, contentTypeId === ContentTypeId.Url ? { url: fallbackUrl } : contentValues);
  const previewValue = saved?.shortUrl ?? existing?.shortUrl ?? fallbackUrl ?? `${REDIRECT_BASE}/preview`;

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
    if (contentTypeId === ContentTypeId.MobileApp && !["appStore", "playStore", "other"].some((k) => (contentValues[k] ?? "").trim())) {
      setError("Add at least one link — App Store, Google Play, or a custom URL for other devices.");
      return;
    }
    setSaving(true);
    // Content shapes the request: static bakes its payload server-side (no redirect, no rules); a self-routed
    // type (mobileApp) is derived server-side from its fields; a plain URL keeps its own routing rules.
    const isStatic = contentDef.mode === ContentMode.Static;
    const selfRouted = contentTypeId === ContentTypeId.MobileApp; // backend derives fallback + device rules from the fields
    const content = buildContent(contentTypeId, contentTypeId === ContentTypeId.Url ? { url: fallbackUrl } : contentValues);
    const request = {
      name: name.trim() || "Untitled code",
      codeType: CodeType.Qr,
      barcodeFormat: symbology,
      // Static bakes a payload; mobileApp's fallback + rules are derived on the server — both send empty here.
      fallbackUrl: isStatic || selfRouted ? "" : fallbackUrl.trim(),
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
      setExisting(dto);
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
                existing={existing}
                name={name}
                onNameChange={setName}
                contentTypeId={contentTypeId}
                onContentTypeIdChange={setContentTypeId}
                fallbackUrl={fallbackUrl}
                onFallbackUrlChange={setFallbackUrl}
                contentValues={contentValues}
                onContentValuesChange={setContentValues}
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
        <Card className="surface-soft flex flex-col items-center gap-4 p-6 lg:sticky lg:top-6 lg:self-start">
          <QrPreview value={previewValue} content={previewContent} codeType={previewCodeType} style={previewStyle} />
          <Text size={SizePreset.Xs} color="muted" align="center">
            Live preview — the final asset rendered server-side (vector-first), so what you see
            is what you download.
          </Text>

          {/* Scannability note — lives under the preview (not in the form) so it reads against the actual render. */}
          <div className="w-full">
            <ContrastCallout
              foreground={foreground}
              background={background}
              transparent={transparentBackground}
              gradient={gradient}
            />
          </div>

          {saved && (
            <Surface
              variant={SurfaceVariant.Subtle}
              tone={ColorTone.Neutral}
              radius="lg"
              padding="md"
              className="w-full"
            >
              <Text size={SizePreset.Sm} weight="medium" role="status">{isEdit ? "Changes saved ✓" : "Code created ✓"}</Text>
              {isDynamicContent(saved.content) ? (
                <Text size={SizePreset.Sm} color="muted" isTruncated className="mt-1" title={saved.shortUrl}>
                  {saved.shortUrl}
                </Text>
              ) : (
                <Text size={SizePreset.Sm} color="muted" className="mt-1">
                  Payload baked into the code — it works offline, with no redirect.
                </Text>
              )}
              <div className="mt-3 flex flex-wrap items-center gap-2">
                {isDynamicContent(saved.content) && (
                  <CopyButton size={SizePreset.Sm} text={saved.shortUrl} aria-label="Copy short URL">
                    Copy link
                  </CopyButton>
                )}
                <Button asChild size={SizePreset.Sm} variant={ButtonVariant.Outline} tone={ColorTone.Neutral}>
                  <a href={codeImageUrl(saved.id, ImageFormat.Svg)} target="_blank" rel="noreferrer">
                    SVG
                  </a>
                </Button>
                <Button asChild size={SizePreset.Sm} variant={ButtonVariant.Outline} tone={ColorTone.Neutral}>
                  <a href={codeImageUrl(saved.id, ImageFormat.Png)} target="_blank" rel="noreferrer">
                    PNG
                  </a>
                </Button>
                {isEdit ? (
                  onBack && (
                    <Button size={SizePreset.Sm} variant={ButtonVariant.Ghost} tone={ColorTone.Neutral} onClick={onBack}>
                      Done
                    </Button>
                  )
                ) : (
                  <Button size={SizePreset.Sm} variant={ButtonVariant.Ghost} tone={ColorTone.Neutral} onClick={reset}>
                    Create another
                  </Button>
                )}
              </div>
            </Surface>
          )}
        </Card>
      </Grid>
    </Stack>
  );
}
