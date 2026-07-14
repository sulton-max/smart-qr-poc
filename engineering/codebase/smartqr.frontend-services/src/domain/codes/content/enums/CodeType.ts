/** Defines the coarse code kind — QR, barcode, or a plain forwarder link (mirrors backend `CodeType`). */
export const CodeType = {
  /** Refers to a QR-symbology render. */
  Qr: "qr",
  /** Refers to a 1D/2D barcode render. */
  Barcode: "barcode",
  /** Refers to a plain forwarder link (no rendered symbol). */
  Link: "link",
} as const;

export type CodeType = (typeof CodeType)[keyof typeof CodeType];
