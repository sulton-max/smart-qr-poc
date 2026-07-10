// codes domain — the QR-code model, split by concern:
//   common  — the shared aggregate wire contract (CodeDto · requests · preview) + output ImageFormat
//   content — code identity (name / CodeType / BarcodeFormat), the typed CodeContent union, field registry, value↔content ops
//   style   — module / finder shapes, fill, ECC, gradient, emoji + the PreviewStyle aggregate
//   rules   — routing condition types + the RuleDraft builder row
export * from "./common";
export * from "./content";
export * from "./style";
export * from "./rules";
