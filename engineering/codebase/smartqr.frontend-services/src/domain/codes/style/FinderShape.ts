/**
 * Defines a finder (positioning) marker shape — the outer eye frame (`finderShape`)
 * or the inner eye pupil (`finderDotShape`). Mirrors the backend; default `square`.
 */
export const FinderShape = {
  /** Refers to a square eye. */
  Square: "square",
  /** Refers to an eye with rounded corners. */
  Rounded: "rounded",
  /** Refers to a fully circular eye. */
  Circle: "circle",
} as const;

export type FinderShape = (typeof FinderShape)[keyof typeof FinderShape];
