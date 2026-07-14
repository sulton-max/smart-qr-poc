/** Defines a finder (eye) marker shape — the outer frame or the inner pupil (mirrors the backend). */
export const FinderShape = {
  /** Refers to a square eye. */
  Square: "square",
  /** Refers to an eye with rounded corners. */
  Rounded: "rounded",
  /** Refers to a fully circular eye. */
  Circle: "circle",
} as const;

export type FinderShape = (typeof FinderShape)[keyof typeof FinderShape];
