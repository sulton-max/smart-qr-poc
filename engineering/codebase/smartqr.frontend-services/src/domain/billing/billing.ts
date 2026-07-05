// Billing — mirrors SmartQr.Api Billing DTOs + SmartQr.Common.Domain.Billing.Enums.

// Wire enum (camelCase). The DB stores PascalCase via EF `HaveConversion<string>` — a separate
// storage concern from this JSON wire form.
export const Plan = {
  Free: "free",
  Solo: "solo",
  Pro: "pro",
  Agency: "agency",
} as const;
export type Plan = (typeof Plan)[keyof typeof Plan];

/** Human-readable labels for Plan. */
export const PlanLabels: Record<Plan, string> = {
  [Plan.Free]: "Free",
  [Plan.Solo]: "Solo",
  [Plan.Pro]: "Pro",
  [Plan.Agency]: "Agency",
};

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
