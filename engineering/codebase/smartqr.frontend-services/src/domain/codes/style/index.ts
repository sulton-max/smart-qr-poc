// codes/style domain — the visual model: module/finder shapes, fill type, ECC level, gradient, and the
// PreviewStyle aggregate (incl. logo + emoji overlays). Gradient is re-exported from the UI color domain.
export * from "./ModuleShape";
export * from "./FinderShape";
export * from "./FillType";
export * from "./EccLevel";
export * from "./PreviewStyle";
export { Gradient, GradientType } from "@wow-two-beta/ui/domain/color";
export type { GradientStop, LinearGradient, RadialGradient } from "@wow-two-beta/ui/domain/color";
