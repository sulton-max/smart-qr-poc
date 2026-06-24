import { useEffect, useRef, useState } from "react";
import { Spinner } from "@wow-two-beta/ui/feedback";
import { previewCode } from "../api";
import type { CodeType, PreviewStyle } from "../types";

export interface QrPreviewProps {
  /** Data encoded into the code — the short link on edit, a sample URL on create. */
  value: string;
  /** Coarse code kind; derived from the chosen symbology in the builder. */
  codeType: CodeType;
  /** Visual style sent to the server renderer. */
  style: PreviewStyle;
  /** Rendered box edge in px. */
  size?: number;
  /** Debounce window before firing the preview request (ms). */
  debounceMs?: number;
}

/**
 * Live builder preview — renders the **backend-emitted SVG** (server-authoritative
 * parity with the downloadable asset) via `POST /api/codes/preview`. The request is
 * debounced so it isn't fired per-keystroke, and superseded requests are aborted.
 */
export function QrPreview({
  value,
  codeType,
  style,
  size = 240,
  debounceMs = 280,
}: QrPreviewProps) {
  const [svg, setSvg] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  // Serialize style so the effect re-runs on any individual field change.
  const styleKey = JSON.stringify(style);
  // Track the latest in-flight controller so we can abort superseded requests.
  const controllerRef = useRef<AbortController | null>(null);

  useEffect(() => {
    const timer = setTimeout(() => {
      controllerRef.current?.abort();
      const controller = new AbortController();
      controllerRef.current = controller;

      setLoading(true);
      setError(false);

      previewCode({ value: value || " ", codeType, style }, controller.signal)
        .then((markup) => {
          if (controller.signal.aborted) return;
          setSvg(markup);
          setLoading(false);
        })
        .catch((e: unknown) => {
          // Abort is expected when a newer request supersedes this one — ignore it.
          if (controller.signal.aborted || (e instanceof DOMException && e.name === "AbortError")) {
            return;
          }
          setError(true);
          setLoading(false);
        });
    }, debounceMs);

    return () => clearTimeout(timer);
    // styleKey stands in for the deep `style` object.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [value, codeType, styleKey, debounceMs]);

  // Cancel any in-flight request on unmount.
  useEffect(() => () => controllerRef.current?.abort(), []);

  const frameBackground = style.transparentBackground ? "transparent" : style.backgroundColor;

  return (
    <div
      className="relative inline-flex items-center justify-center rounded-xl p-4 [&>svg]:block [&>svg]:h-full [&>svg]:w-full"
      style={{ width: size, height: size, backgroundColor: frameBackground }}
    >
      {svg && (
        // Backend SVG is generated from a trusted first-party endpoint (same origin).
        <div
          className="h-full w-full [&>svg]:block [&>svg]:h-full [&>svg]:w-full"
          dangerouslySetInnerHTML={{ __html: svg }}
        />
      )}

      {loading && (
        <div className="absolute inset-0 flex items-center justify-center rounded-xl bg-white/60 backdrop-blur-[1px] dark:bg-black/40">
          <Spinner size="md" label="Rendering preview" />
        </div>
      )}

      {error && !loading && (
        <div className="absolute inset-0 flex items-center justify-center rounded-xl p-4 text-center text-xs text-[var(--color-fg-muted,#71717a)]">
          Preview unavailable
        </div>
      )}
    </div>
  );
}
