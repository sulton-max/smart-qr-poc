import { useState } from "react";
import { Button, CopyButton } from "@wow-two-beta/ui/actions";
import { ColorField, FormField, Select, TextInput } from "@wow-two-beta/ui/forms";
import { Card, Heading } from "@wow-two-beta/ui/display";
import { QrPreview } from "../components/QrPreview";
import { RuleBuilder } from "../components/RuleBuilder";
import { codeImageUrl, createCode, REDIRECT_BASE } from "../api";
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

export function CreateCodeScreen() {
  const [name, setName] = useState("");
  const [fallbackUrl, setFallbackUrl] = useState("https://example.com");
  const [symbology, setSymbology] = useState<BarcodeFormat>(BarcodeFormat.QrCode);
  const [foreground, setForeground] = useState("#18181b");
  const [background, setBackground] = useState("#ffffff");
  const [rules, setRules] = useState<RuleDraft[]>([]);
  const [creating, setCreating] = useState(false);
  const [created, setCreated] = useState<CodeDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  const previewValue = created?.shortUrl ?? fallbackUrl ?? `${REDIRECT_BASE}/preview`;

  async function handleCreate() {
    setCreating(true);
    setError(null);
    try {
      const dto = await createCode({
        name: name.trim() || "Untitled code",
        codeType: "Qr",
        barcodeFormat: symbology,
        fallbackUrl: fallbackUrl.trim(),
        rules: rules.map((r) => ({
          order: r.order,
          conditionType: r.conditionType,
          conditionValue: r.conditionValue.trim(),
          destination: r.destination.trim(),
        })),
      });
      setCreated(dto);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Something went wrong");
    } finally {
      setCreating(false);
    }
  }

  function reset() {
    setCreated(null);
    setError(null);
  }

  return (
    <div className="flex flex-col gap-6">
      <div>
        <Heading level={1} className="text-2xl font-bold">
          Create a code
        </Heading>
        <p className="text-muted-foreground">
          One code, many destinations — and it never expires.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* ── Builder ── */}
        <Card className="flex flex-col gap-5 p-6">
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
              selected={symbology}
              onChange={(opt) => opt && setSymbology(opt.itemKey)}
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
              <ColorField value={foreground} onChange={(hex) => setForeground(hex ?? "#000000")} />
            </FormField>
            <FormField label="Background">
              <ColorField value={background} onChange={(hex) => setBackground(hex ?? "#ffffff")} />
            </FormField>
          </div>

          <div className="flex flex-col gap-2">
            <span className="text-sm font-medium">Routing rules</span>
            <RuleBuilder rules={rules} onChange={setRules} />
          </div>

          <Button
            tone="primary"
            isFullWidth
            isLoading={creating}
            loadingText="Creating…"
            onClick={handleCreate}
          >
            Create code
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

          {created && (
            <div className="w-full rounded-lg border border-border bg-muted/30 p-4">
              <p className="text-sm font-medium">Code created ✓</p>
              <p className="mt-1 truncate text-sm text-muted-foreground" title={created.shortUrl}>
                {created.shortUrl}
              </p>
              <div className="mt-3 flex flex-wrap items-center gap-2">
                <CopyButton size="sm" text={created.shortUrl} aria-label="Copy short URL">
                  Copy link
                </CopyButton>
                <Button asChild size="sm" variant="outline" tone="neutral">
                  <a href={codeImageUrl(created.id, "svg")} target="_blank" rel="noreferrer">
                    SVG
                  </a>
                </Button>
                <Button asChild size="sm" variant="outline" tone="neutral">
                  <a href={codeImageUrl(created.id, "png")} target="_blank" rel="noreferrer">
                    PNG
                  </a>
                </Button>
                <Button size="sm" variant="ghost" tone="neutral" onClick={reset}>
                  Create another
                </Button>
              </div>
            </div>
          )}
        </Card>
      </div>
    </div>
  );
}
