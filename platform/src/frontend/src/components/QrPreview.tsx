import { QRCodeSVG } from "qrcode.react";

export interface QrPreviewProps {
  /** Encoded payload — the short URL the printed code resolves through. */
  value: string;
  foreground: string;
  background: string;
  /** Error-correction level. Q/H tolerate a center logo. */
  level?: "L" | "M" | "Q" | "H";
  size?: number;
}

/**
 * Domain component (lives in-project; not in the beta lib). Renders the QR live + client-side
 * for instant restyle feedback. The final downloadable asset is produced server-side
 * (vector-first via QRCoder) — this is the editing preview only.
 */
export function QrPreview({
  value,
  foreground,
  background,
  level = "Q",
  size = 240,
}: QrPreviewProps) {
  return (
    <div
      className="inline-flex items-center justify-center rounded-xl p-4"
      style={{ backgroundColor: background }}
    >
      <QRCodeSVG
        value={value || " "}
        size={size}
        level={level}
        fgColor={foreground}
        bgColor={background}
        marginSize={2}
      />
    </div>
  );
}
