import { ColorTone, SizePreset, SurfaceVariant } from "@wow-two-beta/ui/foundation/utils";
import { Button, ButtonVariant, CopyButton } from "@wow-two-beta/ui/presentation/actions";
import { Card, Text } from "@wow-two-beta/ui/presentation/display";
import { Surface } from "@wow-two-beta/ui/presentation/layout";
import { ContentMode, ImageFormat, type CodeDto, type CodeRuleDto, type BarcodeFormat, type CodeStyleDto, type Gradient } from "@/domain/codes";
import { codesApiClient } from "@/integration/codes";
import { ContrastCallout } from "@/presentation/codes/design/components/ContrastCallout";
import { QrPreview } from "../components/QrPreview";

/** Defines props for the persistent preview column — the live render, scannability note, and post-save actions. */
export interface PreviewViewProps {
  /** The value the preview encodes (short link on edit, sample URL on create). */
  readonly previewMode: ContentMode;

  /** The typed content the server bakes into the preview (static types), or the dynamic fallback. */
  readonly previewRules: readonly CodeRuleDto[];

  /** The symbology driving the render — `QrCode` renders the styled path, any other format a plain barcode. */
  readonly previewBarcodeFormat: BarcodeFormat;

  /** The visual style driving the render. */
  readonly previewStyle: CodeStyleDto;

  /** The solid foreground color — feeds the contrast check. */
  readonly foreground: string;

  /** The background color — feeds the contrast check. */
  readonly background: string;

  /** Whether the background is transparent — feeds the contrast check. */
  readonly transparentBackground: boolean;

  /** The foreground gradient (or null) — feeds the contrast check. */
  readonly gradient: Gradient | null;

  /** The saved code after a successful create/update; null before the first save. */
  readonly saved: CodeDto | null;

  /** True in edit mode (switches the confirmation copy + the trailing action). */
  readonly isEdit: boolean;

  /** Fires when the user is done editing (edit mode only). */
  readonly onBack?: () => void;

  /** Fires when the user starts another code (create mode only). */
  readonly onCreateAnother: () => void;
}

/** Renders the builder's right column: the live server-rendered preview, the scannability callout, and — once saved — the short link + downloads. */
export function PreviewView({
  previewMode,
  previewRules,
  previewBarcodeFormat,
  previewStyle,
  foreground,
  background,
  transparentBackground,
  gradient,
  saved,
  isEdit,
  onBack,
  onCreateAnother,
}: PreviewViewProps) {
  return (
    <Card className="surface-soft flex flex-col items-center gap-4 p-6 lg:sticky lg:top-6 lg:self-start">
      <QrPreview mode={previewMode} rules={previewRules} barcodeFormat={previewBarcodeFormat} style={previewStyle} />
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
          {saved.mode === ContentMode.Dynamic && saved.shortUrl ? (
            <Text size={SizePreset.Sm} color="muted" isTruncated className="mt-1" title={saved.shortUrl}>
              {saved.shortUrl}
            </Text>
          ) : (
            <Text size={SizePreset.Sm} color="muted" className="mt-1">
              Payload baked into the code — it works offline, with no redirect.
            </Text>
          )}
          <div className="mt-3 flex flex-wrap items-center gap-2">
            {saved.mode === ContentMode.Dynamic && saved.shortUrl && (
              <CopyButton size={SizePreset.Sm} text={saved.shortUrl} aria-label="Copy short URL">
                Copy link
              </CopyButton>
            )}
            <Button asChild size={SizePreset.Sm} variant={ButtonVariant.Outline} tone={ColorTone.Neutral}>
              <a href={codesApiClient.imageUrl(saved.id, ImageFormat.Svg)} target="_blank" rel="noreferrer">
                SVG
              </a>
            </Button>
            <Button asChild size={SizePreset.Sm} variant={ButtonVariant.Outline} tone={ColorTone.Neutral}>
              <a href={codesApiClient.imageUrl(saved.id, ImageFormat.Png)} target="_blank" rel="noreferrer">
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
              <Button size={SizePreset.Sm} variant={ButtonVariant.Ghost} tone={ColorTone.Neutral} onClick={onCreateAnother}>
                Create another
              </Button>
            )}
          </div>
        </Surface>
      )}
    </Card>
  );
}
