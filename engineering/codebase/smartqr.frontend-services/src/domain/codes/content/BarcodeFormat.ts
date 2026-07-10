/** Defines the symbology a code is rendered as (mirrors backend `barcodeFormat`). */
export const BarcodeFormat = {
  /** Refers to a 2D QR code. */
  QrCode: "qrCode",
  /** Refers to a 2D Data Matrix code. */
  DataMatrix: "dataMatrix",
  /** Refers to a 2D PDF417 stacked barcode. */
  Pdf417: "pdf417",
  /** Refers to a 2D Aztec code. */
  Aztec: "aztec",
  /** Refers to a 1D Code 128 barcode. */
  Code128: "code128",
  /** Refers to a 1D EAN-13 retail barcode. */
  Ean13: "ean13",
  /** Refers to a 1D UPC-A retail barcode. */
  UpcA: "upcA",
} as const;

export type BarcodeFormat = (typeof BarcodeFormat)[keyof typeof BarcodeFormat];
