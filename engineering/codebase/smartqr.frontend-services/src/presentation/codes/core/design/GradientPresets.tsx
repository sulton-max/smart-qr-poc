import type { ReactNode } from "react";
import { ArrowDown, ArrowDownLeft, ArrowDownRight, ArrowRight } from "lucide-react";

import { RadiusGlyph } from "@wow-two-beta/ui/presentation/display";

import { GradientType } from "@/domain/codes/core";

/** Preset-icon glyph size (px). */
export const PRESET_ICON_SIZE = 16;

/** Defines one selectable preset within a gradient projection row. */
export interface GradientPreset {
  /** The value the preset applies — angle in degrees, or radius extent. */
  readonly value: number;

  /** The accessible label for the preset tile. */
  readonly label: string;

  /** The preset's glyph. */
  readonly glyph: ReactNode;
}

/** Defines one projection row — a gradient type's presets + accessible group name. */
export interface PresetRow {
  /** The gradient type this row drives (also gates its enabled state). */
  readonly type: GradientType;

  /** The group's accessible name. */
  readonly ariaLabel: string;

  /** The row's selectable presets. */
  readonly presets: ReadonlyArray<GradientPreset>;
}

/** The linear (angle) projection presets. */
export const LINEAR_PRESETS: PresetRow = {
  type: GradientType.Linear,
  ariaLabel: "Gradient angle",
  presets: [
    { value: 0, label: "Left to right (0°)", glyph: <ArrowRight size={PRESET_ICON_SIZE} /> },
    { value: 45, label: "Diagonal down-right (45°)", glyph: <ArrowDownRight size={PRESET_ICON_SIZE} /> },
    { value: 90, label: "Top to bottom (90°)", glyph: <ArrowDown size={PRESET_ICON_SIZE} /> },
    { value: 135, label: "Diagonal down-left (135°)", glyph: <ArrowDownLeft size={PRESET_ICON_SIZE} /> },
  ],
};

/** The radial (radius) projection presets — extent `0..1`, frontend-only until the backend wires it. */
export const RADIAL_PRESETS: PresetRow = {
  type: GradientType.Radial,
  ariaLabel: "Gradient radius",
  presets: [
    { value: 0.4, label: "Tight radius", glyph: <RadiusGlyph extent={0.4} /> },
    { value: 0.6, label: "Compact radius", glyph: <RadiusGlyph extent={0.6} /> },
    { value: 0.8, label: "Wide radius", glyph: <RadiusGlyph extent={0.8} /> },
    { value: 1.0, label: "Full radius", glyph: <RadiusGlyph extent={1.0} /> },
  ],
};

/** The projection rows — one per `GradientType`, in render order. */
export const PRESET_ROWS: PresetRow[] = [LINEAR_PRESETS, RADIAL_PRESETS];
