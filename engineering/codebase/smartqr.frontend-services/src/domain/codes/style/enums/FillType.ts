/** Defines the foreground fill mode — a solid color or a two-stop gradient. */
export const FillType = {
  /** Refers to a single solid foreground color. */
  Solid: "solid",
  /** Refers to a two-stop foreground gradient. */
  Gradient: "gradient",
} as const;

export type FillType = (typeof FillType)[keyof typeof FillType];
