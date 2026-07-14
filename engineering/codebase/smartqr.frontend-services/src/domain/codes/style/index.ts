// codes/style domain — the visual model: module/finder shapes, fill type, ECC level, gradient, and the
// CodeStyleDto aggregate (incl. logo + emoji overlays). Gradient is re-exported from the UI color domain.
export * from "./enums";
export * from "./models";
export * from "./defaultCodeStyle";
export { Gradient, GradientType } from "@wow-two-beta/ui/domain/color";
export type { GradientStop, LinearGradient, RadialGradient } from "@wow-two-beta/ui/domain/color";
