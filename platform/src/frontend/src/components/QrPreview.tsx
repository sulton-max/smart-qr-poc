import { QRCodeSVG } from "qrcode.react";

export interface QrPreviewProps {
  value: string;
  foreground: string;
  background: string;
  // Q/H tolerate a center logo.
  level?: "L" | "M" | "Q" | "H";
  size?: number;
}

// Client-side editing preview only; the downloadable asset is rendered server-side via QRCoder.
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
