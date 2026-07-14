import { Gradient } from "@wow-two-beta/ui/domain/color";

import { EccLevel } from "./enums/EccLevel";
import { FinderShape } from "./enums/FinderShape";
import { ModuleShape } from "./enums/ModuleShape";
import type { CodeStyleDto } from "./models";

/** Defines the default style seeding every new code — rounded body + eyes, a black→violet radial gradient, no overlay. */
export const defaultCodeStyle: CodeStyleDto = {
  foregroundColor: "#000000",
  backgroundColor: "#ffffff",
  transparentBackground: false,
  eccLevel: EccLevel.Q,
  quietZoneModules: 2,
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
};
