/** Defines the QR error-correction level (mirrors backend `EccLevel`; camelCase wire). */
export const EccLevel = {
  /** Refers to ~7% recovery (Low). */
  L: "l",
  /** Refers to ~15% recovery (Medium). */
  M: "m",
  /** Refers to ~25% recovery (Quartile). */
  Q: "q",
  /** Refers to ~30% recovery (High). */
  H: "h",
} as const;

export type EccLevel = (typeof EccLevel)[keyof typeof EccLevel];
