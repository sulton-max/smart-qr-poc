import type { BarcodeFormat, ContentMode, CodeRuleDto, CodeStyleDto } from "@/domain/codes";

/** Defines the live-preview request body — renders a code without persisting it. */
export interface CodePreviewApiRequest {
  /** The symbology to render — `qrCode` renders the styled path, any other format a plain barcode. */
  barcodeFormat: BarcodeFormat;

  /** How the symbol resolves, deciding whether the preview bakes the payload or a short link. */
  mode: ContentMode;

  /** The routing rules whose content the preview bakes. */
  rules: CodeRuleDto[];

  /** The visual style to render. */
  style: CodeStyleDto;
}
