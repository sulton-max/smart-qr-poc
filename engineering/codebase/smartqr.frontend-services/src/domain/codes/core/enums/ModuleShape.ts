// QR data-module body shape; mirrors backend `style.moduleShape`. Default `square`.
export const ModuleShape = {
  Square: "square",
  Rounded: "rounded",
  Dots: "dots",
  Classy: "classy",
  ClassyRounded: "classyRounded",
  VerticalBars: "verticalBars",
  HorizontalBars: "horizontalBars",
} as const;
export type ModuleShape = (typeof ModuleShape)[keyof typeof ModuleShape];
