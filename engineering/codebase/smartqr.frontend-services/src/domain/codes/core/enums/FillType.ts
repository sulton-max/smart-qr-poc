/**
 * Defines the foreground fill mode — a solid color vs a two-stop gradient.
 * UI-only: the persisted model carries `gradient | null`, so this never crosses the wire.
 */
export const FillType = {
  /** Refers to a single solid foreground color. */
  Solid: "solid",
  /** Refers to a two-stop foreground gradient. */
  Gradient: "gradient",
} as const;

export type FillType = (typeof FillType)[keyof typeof FillType];
