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
