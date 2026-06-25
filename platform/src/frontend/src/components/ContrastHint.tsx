import { Alert } from "@wow-two-beta/ui/feedback";
import type { PreviewGradient } from "../types";

export interface ContrastHintProps {
  foreground: string;
  background: string;
  transparent: boolean;
  /** When set, contrast is checked against the worst (lightest) gradient stop. */
  gradient: PreviewGradient | null;
}

/** WCAG relative luminance of a `#RRGGBB` color (0 = black, 1 = white). */
function relativeLuminance(hex: string): number {
  const c = hex.replace("#", "");
  if (c.length !== 6) return 0;
  const channel = (i: number) => {
    const v = parseInt(c.slice(i, i + 2), 16) / 255;
    return v <= 0.03928 ? v / 12.92 : Math.pow((v + 0.055) / 1.055, 2.4);
  };
  return 0.2126 * channel(0) + 0.7152 * channel(2) + 0.0722 * channel(4);
}

/** WCAG contrast ratio between two `#RRGGBB` colors (1 = identical, 21 = black-on-white). */
function contrastRatio(a: string, b: string): number {
  const la = relativeLuminance(a);
  const lb = relativeLuminance(b);
  return (Math.max(la, lb) + 0.05) / (Math.min(la, lb) + 0.05);
}

/**
 * Calm, passive scannability guardrail (v0.5) — warns when the foreground↔background contrast is too low to
 * scan reliably, or when the code is inverted (light-on-dark). Never blocks; stays silent unless there's a real risk.
 */
export function ContrastHint({ foreground, background, transparent, gradient }: ContrastHintProps) {
  if (transparent) {
    return (
      <Alert
        severity="info"
        description="Transparent background — make sure the code sits on a light, plain surface so it scans."
      />
    );
  }

  // Worst case across the foreground colors (gradient stops, or the solid foreground) vs the background.
  const fgColors = gradient ? gradient.stops.map((s) => s.color) : [foreground];
  const ratio = Math.min(...fgColors.map((c) => contrastRatio(c, background)));
  const inverted = fgColors.some((c) => relativeLuminance(c) > relativeLuminance(background));
  const shown = ratio.toFixed(1);

  if (inverted) {
    return (
      <Alert
        severity="warning"
        description={`Light-on-dark inverts the code (${shown}:1) — many scanners fail. Darken the foreground or lighten the background.`}
      />
    );
  }
  if (ratio < 3) {
    return (
      <Alert
        severity="warning"
        description={`Very low contrast (${shown}:1) — the code likely won't scan. Darken the foreground or lighten the background.`}
      />
    );
  }
  if (ratio < 4.5) {
    return (
      <Alert
        severity="warning"
        description={`Contrast is a little low (${shown}:1) — test a scan before printing.`}
      />
    );
  }

  return null; // good contrast — stay quiet
}
