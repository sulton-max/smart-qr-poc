/** Defines the QR data-module body shape (mirrors backend `style.moduleShape`). */
export const ModuleShape = {
  /** Refers to a plain square module. */
  Square: "square",
  /** Refers to a module with rounded corners. */
  Rounded: "rounded",
  /** Refers to detached circular dots. */
  Dots: "dots",
  /** Refers to a stylized module with a subtle corner cut. */
  Classy: "classy",
  /** Refers to the classy module with softened (rounded) corners. */
  ClassyRounded: "classyRounded",
  /** Refers to modules fused into vertical bars. */
  VerticalBars: "verticalBars",
  /** Refers to modules fused into horizontal bars. */
  HorizontalBars: "horizontalBars",
} as const;

export type ModuleShape = (typeof ModuleShape)[keyof typeof ModuleShape];
