import { Gradient } from "@wow-two-beta/ui/domain/color";
import { EccLevel } from "./EccLevel";
import { FinderShape } from "./FinderShape";
import { ModuleShape } from "./ModuleShape";
import type { PreviewStyle } from "./PreviewStyle";

/**
 * The default style applied to every new code — rounded body + eyes, a black→violet radial gradient (full
 * radius), no center emoji. A domain constant (not presentation): it seeds the builder so a freshly created
 * code is already polished, and any surface that renders a code can fall back to the same house look.
 */
export const defaultCodeStyle: PreviewStyle = {
  foregroundColor: "#000000",
  backgroundColor: "#ffffff",
  transparentBackground: false,
  eccLevel: EccLevel.Q,
  quietZoneModules: 2,
  logo: null,
  moduleShape: ModuleShape.Rounded,
  finderShape: FinderShape.Rounded,
  finderDotShape: FinderShape.Rounded,
  gradient: Gradient.radial(
    [
      { color: "#000000", offset: 0 },
      { color: "#7c3aed", offset: 1 },
    ],
    1,
  ),
  emoji: null,
};
