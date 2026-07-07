import type { ReactNode } from "react";

import { CellsGlyph, DotsGlyph, FrameGlyph, HorizontalBarsGlyph, VerticalBarsGlyph } from "@wow-two-beta/ui/presentation/display";

import { FinderShape, ModuleShape } from "@/domain/codes/core";

/** Defines the display for a module-shape option. */
interface ModuleShapeDisplay {
  readonly label: string;
  readonly icon: ReactNode;
}

/** Maps each module shape to its label + preview glyph (square-family corner radius folded in). */
export const ModuleShapeDisplays: Record<ModuleShape, ModuleShapeDisplay> = {
  [ModuleShape.Square]: {
    label: "Square",
    icon: <CellsGlyph cornerRx={0} />,
  },
  [ModuleShape.Rounded]: {
    label: "Rounded",
    icon: <CellsGlyph cornerRx={1.6} />,
  },
  [ModuleShape.Dots]: {
    label: "Dots",
    icon: <DotsGlyph />,
  },
  [ModuleShape.Classy]: {
    label: "Classy",
    icon: <CellsGlyph cornerRx={0.8} />,
  },
  [ModuleShape.ClassyRounded]: {
    label: "Classy rounded",
    icon: <CellsGlyph cornerRx={2.2} />,
  },
  [ModuleShape.VerticalBars]: {
    label: "Vertical bars",
    icon: <VerticalBarsGlyph />,
  },
  [ModuleShape.HorizontalBars]: {
    label: "Horizontal bars",
    icon: <HorizontalBarsGlyph />,
  },
};

/** Defines the display for a finder-eye shape option — label + outer-frame / inner-pupil glyphs. */
interface FinderShapeDisplay {
  readonly label: string;
  readonly icon: ReactNode;
  readonly dotIcon: ReactNode;
}

/** Maps each finder shape to its label + outer / inner preview glyphs (frame radius + pupil roundness folded in). */
export const FinderShapeDisplays: Record<FinderShape, FinderShapeDisplay> = {
  [FinderShape.Square]: {
    label: "Square",
    icon: <FrameGlyph frameRx={0} pupilRoundness={0} />,
    dotIcon: <FrameGlyph frameRx={0} pupilRoundness={0} isDot />,
  },
  [FinderShape.Rounded]: {
    label: "Rounded",
    icon: <FrameGlyph frameRx={4} pupilRoundness={0.2} />,
    dotIcon: <FrameGlyph frameRx={4} pupilRoundness={0.2} isDot />,
  },
  [FinderShape.Circle]: {
    label: "Circle",
    icon: <FrameGlyph frameRx={11} pupilRoundness={0.5} />,
    dotIcon: <FrameGlyph frameRx={11} pupilRoundness={0.5} isDot />,
  },
};
