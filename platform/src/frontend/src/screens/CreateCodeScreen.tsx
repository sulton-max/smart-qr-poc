import { useEffect, useMemo, useState } from "react";
import { Button, CopyButton, SegmentedControl, ToggleButton } from "@wow-two-beta/ui/actions";
import { ColorPicker, FormField, Select, SwitchField, TextInput } from "@wow-two-beta/ui/forms";
import { Card, Heading, Text } from "@wow-two-beta/ui/display";
import { Alert, Spinner } from "@wow-two-beta/ui/feedback";
import { Center, Grid, Stack, Surface } from "@wow-two-beta/ui/layout";
import { ArrowLeft } from "lucide-react";
import { QrPreview } from "../components/QrPreview";
import { RuleBuilder } from "../components/RuleBuilder";
import { ShapeControls } from "../components/ShapeControls";
import { GradientControls } from "../components/GradientControls";
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
  const [tab, setTab] = useState<"destination" | "style" | "center" | "routing">("destination");
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
            <ToggleButton value="destination" className="flex-1">Destination</ToggleButton>
            <ToggleButton value="style" className="flex-1">Style</ToggleButton>
            <ToggleButton value="center" className="flex-1">Center</ToggleButton>
            <ToggleButton value="routing" className="flex-1">Routing</ToggleButton>
          </SegmentedControl>

          {/* key={tab} re-mounts on switch so the fade-through re-fires; motion-safe respects reduced-motion. */}
          <div key={tab} className="flex flex-col gap-5 motion-safe:animate-(--animate-fade-in)">
          {tab === "destination" && (
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

          {tab === "style" && (
            <>
              <Text as="span" size="sm" weight="medium">Appearance</Text>
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
              {gradient === null ? (
                <Grid columns="2" gap="4">
                  <FormField label="Foreground">
                    <ColorPicker value={foreground} onValueChange={(hex) => setForeground(hex ?? "#000000")} />
                  </FormField>
                  <FormField label="Background">
                    <ColorPicker value={background} onValueChange={(hex) => setBackground(hex ?? "#ffffff")} />
                  </FormField>
                </Grid>
              ) : (
                // The gradient overrides the solid foreground, so only the background remains here.
                <FormField label="Background" helper="Foreground is set by the gradient below.">
                  <ColorPicker value={background} onValueChange={(hex) => setBackground(hex ?? "#ffffff")} />
                </FormField>
              )}
              <SwitchField
                label="Transparent background"
                description="Drop the background so the code sits on any surface."
                side="right"
                checked={transparentBackground}
                onChange={(e) => setTransparentBackground(e.currentTarget.checked)}
              />
              <GradientControls gradient={gradient} foreground={foreground} onChange={setGradient} />
              <ContrastHint
                foreground={foreground}
                background={background}
                transparent={transparentBackground}
                gradient={gradient}
              />
              <Text as="span" size="sm" weight="medium">Shape</Text>
              <ShapeControls
                moduleShape={moduleShape}
                finderShape={finderShape}
                finderDotShape={finderDotShape}
                onModuleShapeChange={setModuleShape}
                onFinderShapeChange={setFinderShape}
                onFinderDotShapeChange={setFinderDotShape}
              />
            </>
          )}

          {tab === "center" && <EmojiControls emoji={emoji} onChange={setEmoji} />}

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
