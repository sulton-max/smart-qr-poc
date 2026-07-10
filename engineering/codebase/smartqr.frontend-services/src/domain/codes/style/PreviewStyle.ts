// The code's visual style — the shape/fill/ECC knobs plus the optional center overlays — sent with
// every preview + save request. Field names match the pinned wire contract (camelCase), not the
// internal C# `CodeRenderOptions` shape.

import type { Gradient } from "@wow-two-beta/ui/domain/color";
import type { EccLevel } from "./EccLevel";
import type { FinderShape } from "./FinderShape";
import type { ModuleShape } from "./ModuleShape";

// Optional center logo overlay for the preview render.
export interface PreviewLogo {
  // Data URL (e.g. `data:image/png;base64,…`) of the logo image.
  dataUrl: string;
  // Logo edge as a fraction of the code's width (0–1).
  sizeRatio: number;
}

// Optional center emoji overlay for the preview render (no file upload).
export interface DesignEmojiOverlay {
  glyph: string;
  sizeRatio: number; // fraction of the code's width (0–1)
}

// Visual style sent with a preview request. Field names match the pinned wire
// contract (camelCase), not the internal C# `CodeRenderOptions` shape.
export interface PreviewStyle {
  foregroundColor: string; // #RRGGBB
  backgroundColor: string; // #RRGGBB
  transparentBackground: boolean;
  eccLevel: EccLevel;
  quietZoneModules: number;
  logo: PreviewLogo | null;
  moduleShape: ModuleShape; // default "square"
  finderShape: FinderShape; // outer eye frame; default "square"
  finderDotShape: FinderShape; // inner eye pupil; default "square"
  gradient: Gradient | null; // foreground gradient; null = solid foregroundColor
  emoji: DesignEmojiOverlay | null; // center emoji overlay; null = none
}
