import { useEffect, useMemo, useState } from "react";
import { Button, CopyButton, SegmentedControl, ToggleButton } from "@wow-two-beta/ui/actions";
import { FormField, Select, TextInput } from "@wow-two-beta/ui/forms";
import { Accordion, Card, Heading, Text } from "@wow-two-beta/ui/display";
import { Alert, Spinner } from "@wow-two-beta/ui/feedback";
import { Center, Grid, Stack, Surface } from "@wow-two-beta/ui/layout";
import { ArrowLeft } from "lucide-react";
import { QrPreview } from "../components/QrPreview";
import { RuleBuilder } from "../components/RuleBuilder";
import { ShapeControls } from "../components/ShapeControls";
import { FillControls } from "../components/FillControls";
import { TileColorPicker } from "../components/TileColorPicker";
import { EmojiControls } from "../components/EmojiControls";
import { ContrastHint } from "../components/ContrastHint";
import { codeImageUrl, createCode, getCode, updateCode, REDIRECT_BASE } from "../api";
import {
  BarcodeFormat,
  FinderShape,
  GradientType,
  ModuleShape,
  type CodeDto,
  type CodeType,
  type PreviewEmoji,
  type PreviewGradient,
  type PreviewStyle,
  type RuleDraft,
} from "../types";

const SYMBOLOGY_LABEL: Record<BarcodeFormat, string> = {
  QrCode: "QR code",
  DataMatrix: "Data Matrix",
  Pdf417: "PDF417",
  Aztec: "Aztec",
  Code128: "Code 128",
  Ean13: "EAN-13",
  UpcA: "UPC-A",
};

// Force symmetric accordion-panel padding. Installed @wow-two-beta/ui@0.0.64 renders the
// Accordion content as `px-3 pb-3` (zero top padding → content hugs the trigger, 12px gap at
// the bottom). The SDK source already fixes this to `py-3`; this override targets the inner
// padding div and is a redundant no-op once that ui bump lands (safe to drop then).
const PANEL_PADDING = "[&>div>div]:!py-3";

/** Persisted rules → builder draft shape (adds client-side keys). */
function toDrafts(code: CodeDto): RuleDraft[] {
  return code.rules.map((r) => ({
    id: crypto.randomUUID(),
    order: r.order,
    conditionType: r.conditionType,
    conditionValue: r.conditionValue ?? "",
    destination: r.destination,
  }));
}

/** Case-insensitively resolves a wire enum value (written verbatim/PascalCase) to a frontend const value. */
function enumFromWire<T extends string>(values: readonly T[], wire: string, fallback: T): T {
  return values.find((v) => v.toLowerCase() === wire.toLowerCase()) ?? fallback;
}

/** Maps a persisted gradient (wire shape) back to the builder's gradient, or null for a solid foreground. */
function gradientFromDto(g: CodeDto["style"]["gradient"]): PreviewGradient | null {
  if (!g) return null;
  return {
    type: enumFromWire(Object.values(GradientType), g.type, GradientType.Linear),
    angle: g.angle,
    stops: g.stops.map((s) => ({ color: s.color, offset: s.offset })),
  };
}

export interface CreateCodeScreenProps {
  /** Set → edit this code (PUT); unset → create (POST). */
  codeId?: string;
  /** Return to the codes list. */
  onBack?: () => void;
  /** Save succeeded — parent refreshes the list. */
  onSaved?: () => void;
}

/** Code builder — create, or edit when `codeId` set. Edit submits a full replace; slug is read-only (printed, immutable). */
export function CreateCodeScreen({ codeId, onBack, onSaved }: CreateCodeScreenProps) {
  const isEdit = Boolean(codeId);

  const [name, setName] = useState("");
  const [fallbackUrl, setFallbackUrl] = useState("https://example.com");
  const [symbology, setSymbology] = useState<BarcodeFormat>(BarcodeFormat.QrCode);
  const [foreground, setForeground] = useState("#18181b");
  const [background, setBackground] = useState("#ffffff");
  // Code-styling shapes (v0.5) — default `square` so the render is unchanged until picked.
  // Layout D (v0.6): 3 grouped tabs — Content · Design (accordion) · Routing.
  const [tab, setTab] = useState<"content" | "design" | "routing">("content");
  const [moduleShape, setModuleShape] = useState<ModuleShape>(ModuleShape.Square);
  const [finderShape, setFinderShape] = useState<FinderShape>(FinderShape.Square);
  const [finderDotShape, setFinderDotShape] = useState<FinderShape>(FinderShape.Square);
  const [gradient, setGradient] = useState<PreviewGradient | null>(null);
  const [transparentBackground, setTransparentBackground] = useState(false);
  const [emoji, setEmoji] = useState<PreviewEmoji | null>(null);
  const [rules, setRules] = useState<RuleDraft[]>([]);
  const [existing, setExisting] = useState<CodeDto | null>(null);

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
        setModuleShape(enumFromWire(Object.values(ModuleShape), code.style.moduleShape, ModuleShape.Square));
        setFinderShape(enumFromWire(Object.values(FinderShape), code.style.finderShape, FinderShape.Square));
        setFinderDotShape(enumFromWire(Object.values(FinderShape), code.style.finderDotShape, FinderShape.Square));
        setGradient(gradientFromDto(code.style.gradient));
        setTransparentBackground(code.style.transparentBackground);
        setEmoji(code.style.emoji ?? null);
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

  // On edit, encode the code's permanent short link; on create (no code yet), a sample URL.
  const previewValue = saved?.shortUrl ?? existing?.shortUrl ?? fallbackUrl ?? `${REDIRECT_BASE}/preview`;

  // The preview endpoint's coarse kind: QR symbology → "Qr", any other (1D/2D) → "Barcode".
  const previewCodeType: CodeType = symbology === BarcodeFormat.QrCode ? "Qr" : "Barcode";

  // Defaults (ECC / quiet-zone / logo) aren't surfaced in the builder yet — send the renderer's
  // standard defaults. Surface them as inputs in a later iteration. Shapes default to `square`,
  // so the render is unchanged until the user picks a style.
  const previewStyle: PreviewStyle = useMemo(
    () => ({
      foregroundColor: foreground,
      backgroundColor: background,
      transparentBackground,
      eccLevel: "Q",
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
    setSaving(true);
    setError(null);
    const request = {
      name: name.trim() || "Untitled code",
      codeType: "Qr" as const,
      barcodeFormat: symbology,
      fallbackUrl: fallbackUrl.trim(),
      rules: rules.map((r) => ({
        order: r.order,
        conditionType: r.conditionType,
        conditionValue: r.conditionValue.trim(),
        destination: r.destination.trim(),
      })),
      style: previewStyle,
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
        <Spinner size="lg" label="Loading code" />
      </Center>
    );
  }

  return (
    <Stack gap="6">
      <Stack gap="2">
        {onBack && (
          <Button
            variant="ghost"
            tone="neutral"
            size="sm"
            leadingSlot={<ArrowLeft size={16} />}
            className="-ml-2 self-start"
            onClick={onBack}
          >
            Back to codes
          </Button>
        )}
        <div>
          <Heading level={1} size="xl" weight="bold">
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
          <SegmentedControl
            type="single"
            value={tab}
            onValueChange={(v) => v && setTab(v as typeof tab)}
            aria-label="Builder section"
          >
            <ToggleButton value="content" className="flex-1">Content</ToggleButton>
            <ToggleButton value="design" className="flex-1">Design</ToggleButton>
            <ToggleButton value="routing" className="flex-1">Routing</ToggleButton>
          </SegmentedControl>

          {/* key={tab} re-mounts on switch so the fade-through re-fires; motion-safe respects reduced-motion. */}
          <div key={tab} className="flex flex-col gap-5 motion-safe:animate-(--animate-fade-in)">
          {tab === "content" && (
            <>
              {isEdit && existing && (
                <FormField label="Short link" helper="Encoded into the printed code — permanent and not editable.">
                  <TextInput value={existing.shortUrl} readOnly disabled />
                </FormField>
              )}
              <FormField label="Name">
                <TextInput
                  ring="sm"
                  value={name}
                  placeholder="Spring menu table tent"
                  onChange={(e) => setName(e.target.value)}
                />
              </FormField>
              <FormField label="Fallback URL" helper="Where scans go when no rule matches.">
                <TextInput
                  ring="sm"
                  value={fallbackUrl}
                  placeholder="https://example.com"
                  onChange={(e) => setFallbackUrl(e.target.value)}
                />
              </FormField>
            </>
          )}

          {tab === "design" && (
            <>
              {/* Code type stays outside the accordion — it gates which style options apply (QR vs 1D/2D). */}
              <FormField label="Code type">
                <Select<BarcodeFormat>
                  value={symbology}
                  onValueChange={(opt) => opt && setSymbology(opt.itemKey)}
                >
                  <Select.Trigger>
                    <Select.Value />
                  </Select.Trigger>
                  <Select.Content>
                    {Object.values(BarcodeFormat).map((f) => (
                      <Select.Item key={f} itemKey={f} label={SYMBOLOGY_LABEL[f]} />
                    ))}
                  </Select.Content>
                </Select>
              </FormField>

              {/* Layout D: one styling section open at a time — caps height, scales to any number of sections. */}
              <Accordion
                type="single"
                defaultValue="colors"
                isCollapsible
                className="overflow-hidden rounded-lg border border-border"
              >
                <Accordion.Item value="colors">
                  <Accordion.Trigger>Colors &amp; fill</Accordion.Trigger>
                  <Accordion.Content className={PANEL_PADDING}>
                    <Stack gap="0">
                      <FillControls
                        foreground={foreground}
                        onForegroundChange={setForeground}
                        gradient={gradient}
                        onGradientChange={setGradient}
                      />

                      {/* Background row — a [swatch][Color | Transparent] segmented control.
                          The swatch edits the bg color and greys when Transparent is selected. The
                          swatch is a sibling of the toggle (not nested) — a ColorPicker renders its own
                          <button>, and a button-in-button is invalid/inaccessible markup. */}
                      <div className="flex min-h-10 items-center justify-between gap-4 border-t border-border pt-2">
                        <span className="text-sm text-muted-foreground">Background</span>
                        <div className="flex items-center gap-2">
                          <span className={transparentBackground ? "pointer-events-none opacity-40" : undefined}>
                            <TileColorPicker
                              value={background}
                              onValueChange={setBackground}
                              ariaLabel="Background color"
                              size="md"
                            />
                          </span>
                          <SegmentedControl
                            type="single"
                            value={transparentBackground ? "transparent" : "color"}
                            onValueChange={(v) => {
                              if (v === "color") setTransparentBackground(false);
                              else if (v === "transparent") setTransparentBackground(true);
                            }}
                            aria-label="Background fill"
                          >
                            <ToggleButton value="color" size="sm">Color</ToggleButton>
                            <ToggleButton value="transparent" size="sm">Transparent</ToggleButton>
                          </SegmentedControl>
                        </div>
                      </div>
                    </Stack>
                  </Accordion.Content>
                </Accordion.Item>

                <Accordion.Item value="shape">
                  <Accordion.Trigger>Shape &amp; eyes</Accordion.Trigger>
                  <Accordion.Content className={PANEL_PADDING}>
                    <ShapeControls
                      moduleShape={moduleShape}
                      finderShape={finderShape}
                      finderDotShape={finderDotShape}
                      onModuleShapeChange={setModuleShape}
                      onFinderShapeChange={setFinderShape}
                      onFinderDotShapeChange={setFinderDotShape}
                    />
                  </Accordion.Content>
                </Accordion.Item>

                <Accordion.Item value="center">
                  <Accordion.Trigger>Center</Accordion.Trigger>
                  <Accordion.Content className={PANEL_PADDING}>
                    <EmojiControls emoji={emoji} onChange={setEmoji} />
                  </Accordion.Content>
                </Accordion.Item>
              </Accordion>
            </>
          )}

          {tab === "routing" && <RuleBuilder rules={rules} onChange={setRules} />}
          </div>

          <Button
            tone="primary"
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
          <QrPreview value={previewValue} codeType={previewCodeType} style={previewStyle} />
          <Text size="xs" color="muted" align="center">
            Live preview — the final asset rendered server-side (vector-first), so what you see
            is what you download.
          </Text>

          {/* Scannability note — lives under the preview (not in the form) so it reads against the actual render. */}
          <div className="w-full">
            <ContrastHint
              foreground={foreground}
              background={background}
              transparent={transparentBackground}
              gradient={gradient}
            />
          </div>

          {saved && (
            <Surface
              variant="subtle"
              tone="neutral"
              radius="lg"
              padding="md"
              className="w-full"
            >
              <Text size="sm" weight="medium">{isEdit ? "Changes saved ✓" : "Code created ✓"}</Text>
              <Text size="sm" color="muted" isTruncated className="mt-1" title={saved.shortUrl}>
                {saved.shortUrl}
              </Text>
              <div className="mt-3 flex flex-wrap items-center gap-2">
                <CopyButton size="sm" text={saved.shortUrl} aria-label="Copy short URL">
                  Copy link
                </CopyButton>
                <Button asChild size="sm" variant="outline" tone="neutral">
                  <a href={codeImageUrl(saved.id, "svg")} target="_blank" rel="noreferrer">
                    SVG
                  </a>
                </Button>
                <Button asChild size="sm" variant="outline" tone="neutral">
                  <a href={codeImageUrl(saved.id, "png")} target="_blank" rel="noreferrer">
                    PNG
                  </a>
                </Button>
                {isEdit ? (
                  onBack && (
                    <Button size="sm" variant="ghost" tone="neutral" onClick={onBack}>
                      Done
                    </Button>
                  )
                ) : (
                  <Button size="sm" variant="ghost" tone="neutral" onClick={reset}>
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
