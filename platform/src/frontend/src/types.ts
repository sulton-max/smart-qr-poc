// Frontend mirror of the backend contract (SmartQr.Common.Domain enums + Api DTOs).

export const RuleConditionType = {
  Device: "Device",
  Country: "Country",
  Language: "Language",
  TimeOfDay: "TimeOfDay",
} as const;
export type RuleConditionType = (typeof RuleConditionType)[keyof typeof RuleConditionType];

export const BarcodeFormat = {
  QrCode: "QrCode",
  DataMatrix: "DataMatrix",
  Pdf417: "Pdf417",
  Aztec: "Aztec",
  Code128: "Code128",
  Ean13: "Ean13",
  UpcA: "UpcA",
} as const;
export type BarcodeFormat = (typeof BarcodeFormat)[keyof typeof BarcodeFormat];

// Builder row; `id` is client-side for list keys.
export interface RuleDraft {
  id: string;
  order: number;
  conditionType: RuleConditionType;
  conditionValue: string;
  destination: string;
}

export interface CreateCodeRequest {
  name: string;
  codeType: "Qr" | "Barcode" | "Link";
  barcodeFormat: BarcodeFormat;
  fallbackUrl: string;
  rules: Array<{
    order: number;
    conditionType: RuleConditionType;
    conditionValue: string;
    destination: string;
  }>;
}

// `PUT /api/codes/{id}` — full replace; slug, scan count, creation time are server-preserved.
export type UpdateCodeRequest = CreateCodeRequest;

export interface SetActiveRequest {
  isActive: boolean;
}

// ── Preview (POST /api/codes/preview → image/svg+xml) ──────────────────────────
// Server-authoritative render for the builder's live preview — same engine that
// produces the downloadable asset, so the preview is byte-for-byte parity.

// Coarse code kind for the preview endpoint. Derived from the chosen symbology:
// QrCode → "Qr"; every other (1D/2D) symbology → "Barcode".
export type CodeType = "Qr" | "Barcode" | "Link";

// QR error-correction level; mirrors backend `EccLevel` (L/M/Q/H).
export type EccLevel = "L" | "M" | "Q" | "H";

// Optional center logo overlay for the preview render.
export interface PreviewLogo {
  // Data URL (e.g. `data:image/png;base64,…`) of the logo image.
  dataUrl: string;
  // Logo edge as a fraction of the code's width (0–1).
  sizeRatio: number;
}

// QR data-module body shape; mirrors backend `style.moduleShape`. Default `square`.
export const ModuleShape = {
  Square: "square",
  Rounded: "rounded",
  Dots: "dots",
  Classy: "classy",
  ClassyRounded: "classyRounded",
  VerticalBars: "verticalBars",
  HorizontalBars: "horizontalBars",
} as const;
export type ModuleShape = (typeof ModuleShape)[keyof typeof ModuleShape];

// Finder (positioning) marker shapes. `finderShape` = the outer eye frame,
// `finderDotShape` = the inner eye pupil. Both mirror the backend, default `square`.
export const FinderShape = {
  Square: "square",
  Rounded: "rounded",
  Circle: "circle",
} as const;
export type FinderShape = (typeof FinderShape)[keyof typeof FinderShape];

// Visual style sent with a preview request. Field names match the pinned wire
// contract (camelCase), not the internal C# `CodeRenderOptions` shape.
export interface PreviewStyle {
  foregroundColor: string; // #RRGGBB
  backgroundColor: string; // #RRGGBB
  transparentBackground: boolean;
  eccLevel: EccLevel;
  quietZoneModules: number;
  logo: PreviewLogo | null;
  moduleShape: ModuleShape; // default "square"
  finderShape: FinderShape; // outer eye frame; default "square"
  finderDotShape: FinderShape; // inner eye pupil; default "square"
}

export interface PreviewRequest {
  // The data encoded into the code — the short link on edit, a sample URL on create.
  value: string;
  codeType: CodeType;
  style: PreviewStyle;
}

export interface CodeDto {
  id: string;
  slug: string;
  shortUrl: string;
  name: string;
  codeType: string;
  barcodeFormat: BarcodeFormat;
  fallbackUrl: string;
  isActive: boolean;
  neverExpires: boolean;
  scanCount: number;
  createdAt: string;
  rules: Array<{
    order: number;
    conditionType: RuleConditionType;
    conditionValue: string | null;
    destination: string;
  }>;
}

// Backend `ApiResponse<T>.Success` envelope (camelCased).
export interface ApiSuccess<T> {
  data: T;
}

// Identity
export type UserKind = "Anonymous" | "Guest" | "User";

export interface UserSummary {
  id: string;
  name: string;
  email: string;
}

export interface Me {
  kind: UserKind;
  user: UserSummary | null;
}

// ── Billing (mirrors SmartQr.Api Billing DTOs + SmartQr.Common.Domain.Billing.Enums) ──

// Enum-as-text; matches `Plan.cs` (`HaveConversion<string>`).
export const Plan = {
  Free: "Free",
  Solo: "Solo",
  Pro: "Pro",
  Agency: "Agency",
} as const;
export type Plan = (typeof Plan)[keyof typeof Plan];

// Paid plans in upgrade order; the only ones `/api/billing/checkout` accepts (`Free` rejected).
export const PAID_PLANS: Plan[] = [Plan.Solo, Plan.Pro, Plan.Agency];

export interface CheckoutRequest {
  plan: Plan;
}

// `CheckoutSessionDto` / `PortalSessionDto` — a single hosted Stripe URL.
export interface SessionUrlDto {
  url: string;
}

// `maxCodes === -1` is the Agency unlimited sentinel (render as ∞).
export interface LimitsDto {
  maxCodes: number;
}

export interface UsageDto {
  codeCount: number;
}

// `GET /api/billing/me`; a guest with no subscription row resolves to `{ plan: Free, status: "active" }`.
export interface BillingStatus {
  plan: Plan;
  status: string;
  limits: LimitsDto;
  usage: UsageDto;
}
