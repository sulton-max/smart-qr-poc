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

/** A routing rule row in the builder (client-side `id` for list keys). */
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

/**
 * Body for `PUT /api/codes/{id}` — full replace of the editable fields plus the entire rule set.
 * Mirrors `CreateCodeRequest`; the slug, scan count, and creation time are server-preserved.
 */
export type UpdateCodeRequest = CreateCodeRequest;

/** Body for `PATCH /api/codes/{id}/active` — toggles only `is_active`. */
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

/** Backend `ApiResponse<T>.Success` envelope (camelCased). */
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

/** Subscription plan — enum-as-text, matches `Plan.cs` (`HaveConversion<string>`). */
export const Plan = {
  Free: "Free",
  Solo: "Solo",
  Pro: "Pro",
  Agency: "Agency",
} as const;
export type Plan = (typeof Plan)[keyof typeof Plan];

/** The paid plans, in upgrade order — the only ones `POST /api/billing/checkout` accepts (`Free` is rejected). */
export const PAID_PLANS: Plan[] = [Plan.Solo, Plan.Pro, Plan.Agency];

/**
 * Body for `POST /api/billing/checkout`. `plan` is enum-as-text (the configured
 * `JsonStringEnumConverter`); the backend rejects `Free`.
 */
export interface CheckoutRequest {
  plan: Plan;
}

/** `CheckoutSessionDto` / `PortalSessionDto` — both carry a single hosted Stripe URL to redirect to. */
export interface SessionUrlDto {
  url: string;
}

/** `LimitsDto` — code cap for the plan. `maxCodes === -1` is the Agency unlimited sentinel (render as ∞). */
export interface LimitsDto {
  maxCodes: number;
}

/** `UsageDto` — live count of the caller's codes. */
export interface UsageDto {
  codeCount: number;
}

/**
 * `BillingStatusDto` from `GET /api/billing/me` — plan + raw subscription status
 * + limits + usage. A guest with no subscription row resolves to `{ plan: Free, status: "active" }`.
 */
export interface BillingStatus {
  plan: Plan;
  status: string;
  limits: LimitsDto;
  usage: UsageDto;
}
