// Foreground gradient projection; mirrors backend `GradientType`. Sent lowercase —
// the server reads enum names case-insensitively, but writes them verbatim (PascalCase),
// so values read back from a response are normalized case-insensitively on the client.
export const GradientType = {
  Linear: "linear",
  Radial: "radial",
} as const;
export type GradientType = (typeof GradientType)[keyof typeof GradientType];
