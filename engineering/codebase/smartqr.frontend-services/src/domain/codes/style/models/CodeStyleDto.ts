import type { Gradient } from "@wow-two-beta/ui/domain/color";

import type { EccLevel } from "../EccLevel";
import type { FinderShape } from "../FinderShape";
import type { ModuleShape } from "../ModuleShape";

/** Represents an optional center logo overlay baked into the code render. */
export interface CodeLogoDto {
  /** The logo image as a data URL. */
  dataUrl: string;

  /** The logo edge as a fraction of the code width (0–1). */
  sizeRatio: number;
}

/** Represents an optional center emoji overlay baked into the code render. */
export interface CodeEmojiDto {
  /** The emoji glyph. */
  glyph: string;

  /** The glyph edge as a fraction of the code width (0–1). */
  sizeRatio: number;
}

/** Represents a code's visual style — colors, shapes, ECC, and the optional center overlays. */
export interface CodeStyleDto {
  /** The foreground color (#RRGGBB). */
  foregroundColor: string;

  /** The background color (#RRGGBB). */
  backgroundColor: string;

  /** Whether the background is transparent. */
  transparentBackground: boolean;

  /** The error-correction level. */
  eccLevel: EccLevel;

  /** The quiet-zone width in modules. */
  quietZoneModules: number;

  /** The optional center logo overlay. */
  logo?: CodeLogoDto;

  /** The data-module shape. */
  moduleShape: ModuleShape;

  /** The finder (eye) frame shape. */
  finderShape: FinderShape;

  /** The finder (eye) pupil shape. */
  finderDotShape: FinderShape;

  /** The optional foreground gradient (solid foreground when absent). */
  gradient?: Gradient;

  /** The optional center emoji overlay. */
  emoji?: CodeEmojiDto;
}
