import type { BarcodeFormat, CodeContent, CodeStyleDto } from "@/domain/codes";

/** Defines the live-preview request body — renders a code without persisting it. */
export interface CodePreviewApiRequest {
  /** The fallback data when content is dynamic or absent. */
  value: string;

  /** The symbology to render — `QrCode` renders the styled path, any other format a plain barcode. */
  barcodeFormat: BarcodeFormat;

  /** The visual style to render. */
  style: CodeStyleDto;

  /** The typed content to bake into the preview, when present. */
  content?: CodeContent;
}
