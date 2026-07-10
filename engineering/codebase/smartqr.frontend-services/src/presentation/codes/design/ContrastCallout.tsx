import { Alert } from "@wow-two-beta/ui/presentation/feedback";
import type { Gradient } from "@/domain/codes/style";

/** @internal sRGB channel value at/below which the WCAG luminance curve stays linear. */
const LinearChannelThreshold = 0.03928;

/** @internal WCAG divisor for the linear (low) segment of the luminance curve. */
const LinearChannelDivisor = 12.92;

/** @internal WCAG exponent for the gamma (high) segment of the luminance curve. */
const GammaExponent = 2.4;

/** @internal WCAG relative-luminance weight for the red channel. */
const RedLuminanceCoefficient = 0.2126;

/** @internal WCAG relative-luminance weight for the green channel. */
const GreenLuminanceCoefficient = 0.7152;

/** @internal WCAG relative-luminance weight for the blue channel. */
const BlueLuminanceCoefficient = 0.0722;

/** @internal WCAG contrast ratio below which the code likely won't scan at all. */
const ContrastLow = 3;

/** @internal WCAG AA contrast ratio below which a scan should be tested before printing. */
const ContrastWarn = 4.5;

export interface ContrastCalloutProps {
  readonly foreground: string;

  readonly background: string;

  readonly transparent: boolean;

  /** The foreground gradient; when set, contrast is checked against the worst (lightest) stop. */
  readonly gradient: Gradient | null;
}

/** Computes the WCAG relative luminance of a `#RRGGBB` color (0 = black, 1 = white). */
function relativeLuminance(hex: string): number {
  const c = hex.replace("#", "");
  if (c.length !== 6) return 0;
  const channel = (i: number) => {
    const v = parseInt(c.slice(i, i + 2), 16) / 255;
    return v <= LinearChannelThreshold ? v / LinearChannelDivisor : Math.pow((v + 0.055) / 1.055, GammaExponent);
  };
  return (
    RedLuminanceCoefficient * channel(0) +
    GreenLuminanceCoefficient * channel(2) +
    BlueLuminanceCoefficient * channel(4)
  );
}

/** Computes the WCAG contrast ratio between two `#RRGGBB` colors (1 = identical, 21 = black-on-white). */
function contrastRatio(a: string, b: string): number {
  const la = relativeLuminance(a);
  const lb = relativeLuminance(b);
  return (Math.max(la, lb) + 0.05) / (Math.min(la, lb) + 0.05);
}

/** Resolves the foreground colors to check — the gradient stops when set, else the solid foreground. */
function extractForegroundColors(foreground: string, gradient: Gradient | null): string[] {
  return gradient ? gradient.stops.map((s) => s.color) : [foreground];
}

/**
 * Renders a calm, passive scannability guardrail (v0.5) — warns when the foreground↔background contrast is
 * too low to scan reliably, or when the code is inverted (light-on-dark). Never blocks; stays silent unless
 * there's a real risk.
 */
export function ContrastCallout({ foreground, background, transparent, gradient }: ContrastCalloutProps) {
  if (transparent) {
    return (
      <Alert
        severity="info"
        description="Transparent background — make sure the code sits on a light, plain surface so it scans."
      />
    );
  }

  // Worst case across the foreground colors (gradient stops, or the solid foreground) vs the background.
  const fgColors = extractForegroundColors(foreground, gradient);
  const ratio = Math.min(...fgColors.map((c) => contrastRatio(c, background)));
  const inverted = fgColors.some((c) => relativeLuminance(c) > relativeLuminance(background));
  const ratioFormatted = ratio.toFixed(1);

  // First matching alert wins, checked most-severe first; message is a function of the formatted ratio.
  const alerts = [
    {
      when: inverted,
      description: `Light-on-dark inverts the code (${ratioFormatted}:1) — many scanners fail. Darken the foreground or lighten the background.`,
    },
    {
      when: ratio < ContrastLow,
      description: `Very low contrast (${ratioFormatted}:1) — the code likely won't scan. Darken the foreground or lighten the background.`,
    },
    {
      when: ratio < ContrastWarn,
      description: `Contrast is a little low (${ratioFormatted}:1) — test a scan before printing.`,
    },
  ];

  const alert = alerts.find((a) => a.when);
  if (alert) {
    return <Alert severity="warning" description={alert.description} />;
  }

  return null; // good contrast — stay quiet
}
