// Billing — mirrors SmartQr.Api Billing DTOs + SmartQr.Common.Domain.Billing.Enums.

/**
 * Defines a subscription plan (mirrors backend `SmartQr.Common.Domain.Billing.Enums.Plan`).
 * Wire form is camelCase; the DB stores PascalCase via EF `HaveConversion<string>` — a separate storage concern.
 */
export const Plan = {
  /** Refers to the free tier. */
  Free: "free",
  /** Refers to the paid Solo tier. */
  Solo: "solo",
  /** Refers to the paid Pro tier. */
  Pro: "pro",
  /** Refers to the paid Dev / Agency tier. */
  Agency: "agency",
} as const;

export type Plan = (typeof Plan)[keyof typeof Plan];

// Paid plans in upgrade order; the only ones `/api/billing/checkout` accepts (`Free` rejected).
export const PAID_PLANS: ReadonlyArray<Plan> = [Plan.Solo, Plan.Pro, Plan.Agency];

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
