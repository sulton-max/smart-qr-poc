import { useEffect, useState } from "react";
import { Button, CopyButton } from "@wow-two-beta/ui/actions";
import { ColorField, FormField, Select, TextInput } from "@wow-two-beta/ui/forms";
import { Card, Heading } from "@wow-two-beta/ui/display";
import { Spinner } from "@wow-two-beta/ui/feedback";
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

/** Maps a persisted code's rules onto the builder's draft shape (with client-side keys). */
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
  /** When set, the builder loads this code and edits it (PUT) instead of creating a new one (POST). */
  codeId?: string;
  /** Return to the previous screen (the codes list). */
  onBack?: () => void;
  /** Notify the parent that a save succeeded — used to refresh the list on the way back. */
  onSaved?: () => void;
}

/**
 * Code builder — create mode by default, edit mode when `codeId` is provided. Edit mode prefills
 * every editable field (including the rule set) and submits a full replace via `updateCode`; the
 * slug is shown read-only because it is printed and immutable.
 */
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

  // Edit mode: load the code once and prefill every field.
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
      <div className="flex min-h-[60vh] items-center justify-center">
        <Spinner size="lg" label="Loading code" />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-2">
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
          <Heading level={1} className="text-2xl font-bold">
            {isEdit ? "Edit code" : "Create a code"}
          </Heading>
          <p className="text-muted-foreground">
            {isEdit
              ? "Update the destination and routing — the printed code keeps working."
              : "One code, many destinations — and it never expires."}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* ── Builder ── */}
        <Card className="flex flex-col gap-5 p-6">
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

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Foreground">
              <ColorField value={foreground} onValueChange={(hex) => setForeground(hex ?? "#000000")} />
            </FormField>
            <FormField label="Background">
              <ColorField value={background} onValueChange={(hex) => setBackground(hex ?? "#ffffff")} />
            </FormField>
          </div>

          <div className="flex flex-col gap-2">
            <span className="text-sm font-medium">Routing rules</span>
            <RuleBuilder rules={rules} onChange={setRules} />
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

          {error && (
            <p className="rounded-md border border-destructive/40 bg-destructive/5 p-3 text-sm text-destructive">
              {error}
            </p>
          )}
        </Card>

        {/* ── Preview ── */}
        <Card className="flex flex-col items-center gap-4 p-6">
          <QrPreview value={previewValue} foreground={foreground} background={background} />
          <p className="text-center text-xs text-muted-foreground">
            Live preview (client-side). The final downloadable asset is rendered server-side
            (vector-first).
          </p>

          {saved && (
            <div className="w-full rounded-lg border border-border bg-muted/30 p-4">
              <p className="text-sm font-medium">{isEdit ? "Changes saved ✓" : "Code created ✓"}</p>
              <p className="mt-1 truncate text-sm text-muted-foreground" title={saved.shortUrl}>
                {saved.shortUrl}
              </p>
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
            </div>
          )}
        </Card>
      </div>
    </div>
  );
}
