export const BarcodeFormat = {
  QrCode: "qrCode",
  DataMatrix: "dataMatrix",
  Pdf417: "pdf417",
  Aztec: "aztec",
  Code128: "code128",
  Ean13: "ean13",
  UpcA: "upcA",
} as const;
export type BarcodeFormat = (typeof BarcodeFormat)[keyof typeof BarcodeFormat];

/** Human-readable labels for BarcodeFormat. */
export const BarcodeFormatLabels: Record<BarcodeFormat, string> = {
  [BarcodeFormat.QrCode]: "QR code",
  [BarcodeFormat.DataMatrix]: "Data Matrix",
  [BarcodeFormat.Pdf417]: "PDF417",
  [BarcodeFormat.Aztec]: "Aztec",
  [BarcodeFormat.Code128]: "Code 128",
  [BarcodeFormat.Ean13]: "EAN-13",
  [BarcodeFormat.UpcA]: "UPC-A",
};
