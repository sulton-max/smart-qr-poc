// codes domain — the QR-code model, split by concern:
//   common  — the CodeDto read entity + output ImageFormat
//   content — code identity (name / CodeType / BarcodeFormat), the typed CodeContent union, registry, value↔content ops
//   style   — module / finder shapes, fill, ECC, gradient, emoji + the CodeStyleDto aggregate
//   rules   — routing condition types + the CodeRuleDto wire rule
export * from "./common";
export * from "./content";
export * from "./style";
export * from "./rules";
