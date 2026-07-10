/**
 * Defines the coarse code kind for the preview endpoint — derived from the chosen symbology
 * (QR → `Qr`; any other 1D/2D → `Barcode`). camelCase wire (mirrors backend `CodeType`).
 */
export const CodeType = {
  /** Refers to a QR-symbology render. */
  Qr: "qr",
  /** Refers to a 1D/2D barcode render. */
  Barcode: "barcode",
  /** Refers to a plain forwarder link (no rendered symbol). */
  Link: "link",
} as const;

export type CodeType = (typeof CodeType)[keyof typeof CodeType];
