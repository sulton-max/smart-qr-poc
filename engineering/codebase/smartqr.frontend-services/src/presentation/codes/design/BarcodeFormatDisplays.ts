import { BarcodeFormat } from "@/domain/codes/content";

/** Defines the display for a barcode-format option. */
interface BarcodeFormatDisplay {
  label: string;
}

/** Maps each barcode format to its display. */
export const BarcodeFormatDisplays: Record<BarcodeFormat, BarcodeFormatDisplay> = {
  [BarcodeFormat.QrCode]: { label: "QR code" },
  [BarcodeFormat.DataMatrix]: { label: "Data Matrix" },
  [BarcodeFormat.Pdf417]: { label: "PDF417" },
  [BarcodeFormat.Aztec]: { label: "Aztec" },
  [BarcodeFormat.Code128]: { label: "Code 128" },
  [BarcodeFormat.Ean13]: { label: "EAN-13" },
  [BarcodeFormat.UpcA]: { label: "UPC-A" },
};
