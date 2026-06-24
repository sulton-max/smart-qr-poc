import { useEffect, useState } from "react";
import { Button, CopyButton } from "@wow-two-beta/ui/actions";
import { ColorField, FormField, Select, TextInput } from "@wow-two-beta/ui/forms";
import { Card, Heading, Text } from "@wow-two-beta/ui/display";
import { Alert, Spinner } from "@wow-two-beta/ui/feedback";
import { Center, Grid, Stack, Surface } from "@wow-two-beta/ui/layout";
import { ArrowLeft } from "lucide-react";
import { QrPreview } from "../components/QrPreview";
import { RuleBuilder } from "../components/RuleBuilder";
import { codeImageUrl, createCode, getCode, updateCode, REDIRECT_BASE } from "../api";
import { BarcodeFormat, type CodeDto, type RuleDraft } from "../types";

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

  const previewValue = saved?.shortUrl ?? existing?.shortUrl ?? fallbackUrl ?? `${REDIRECT_BASE}/preview`;

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
        <Card className="flex flex-col gap-5 p-6">
          <Text as="span" size="sm" weight="medium">Destination</Text>
          {isEdit && existing && (
            <FormField label="Short link" helper="Encoded into the printed code — permanent and not editable.">
              <TextInput value={existing.shortUrl} readOnly disabled />
            </FormField>
          )}

          <FormField label="Name">
            <TextInput
              value={name}
              placeholder="Spring menu table tent"
              onChange={(e) => setName(e.target.value)}
            />
          </FormField>

          <FormField label="Fallback URL" helper="Where scans go when no rule matches.">
            <TextInput
              value={fallbackUrl}
              placeholder="https://example.com"
              onChange={(e) => setFallbackUrl(e.target.value)}
            />
          </FormField>

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

          <Grid columns="2" gap="4">
            <FormField label="Foreground">
              <ColorField value={foreground} onValueChange={(hex) => setForeground(hex ?? "#000000")} />
            </FormField>
            <FormField label="Background">
              <ColorField value={background} onValueChange={(hex) => setBackground(hex ?? "#ffffff")} />
            </FormField>
          </Grid>

          <Stack gap="2">
            <Text as="span" size="sm" weight="medium">Routing rules</Text>
            <RuleBuilder rules={rules} onChange={setRules} />
          </Stack>

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
        <Card className="flex flex-col items-center gap-4 p-6 lg:sticky lg:top-6 lg:self-start">
          <QrPreview value={previewValue} foreground={foreground} background={background} />
          <Text size="xs" color="muted" align="center">
            Live preview (client-side). The final downloadable asset is rendered server-side
            (vector-first).
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
