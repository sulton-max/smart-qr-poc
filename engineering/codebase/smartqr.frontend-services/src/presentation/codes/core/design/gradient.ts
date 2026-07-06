import type { ComponentType } from "react";
import { ArrowDown, ArrowDownLeft, ArrowDownRight, ArrowRight } from "lucide-react";

/** Linear-gradient direction presets — angle (deg) paired with a directional arrow. */
export const ANGLES: { value: number; Icon: ComponentType<{ size?: number }>; label: string }[] = [
  { value: 0, Icon: ArrowRight, label: "Left to right" },
  { value: 45, Icon: ArrowDownRight, label: "Diagonal down-right" },
  { value: 90, Icon: ArrowDown, label: "Top to bottom" },
  { value: 135, Icon: ArrowDownLeft, label: "Diagonal down-left" },
];

/** Radial-gradient radius presets (0..1 extent). Frontend-only until the backend wires it. */
export const RADII: { value: number; label: string }[] = [
  { value: 0.4, label: "Tight" },
  { value: 0.6, label: "Compact" },
  { value: 0.8, label: "Wide" },
  { value: 1.0, label: "Full" },
];

export const DEFAULT_ANGLE = 45;
export const DEFAULT_RADIUS = 0.8;

/** Preset-icon glyph size (px). */
export const PRESET_ICON_SIZE = 16;

/** Default gradient end stop — the brand violet (matches `--color-primary`). A stored data value, so a concrete hex, not a CSS token. */
export const DEFAULT_GRADIENT_END = "#7c3aed";
