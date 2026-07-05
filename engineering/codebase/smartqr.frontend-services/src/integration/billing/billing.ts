import type { BillingStatus, Plan, SessionUrlDto } from "@/domain/billing";
import { API_BASE, problemError, readData } from "../common/client";

// Owner-scoped; a guest with no subscription row resolves to Free/active.
export async function getBilling(): Promise<BillingStatus> {
  const res = await fetch(`${API_BASE}/api/billing/me`, {
    credentials: "include",
  });

  if (!res.ok) throw await problemError(res, "Billing status failed");
  return readData<BillingStatus>(res);
}

// Returns a Stripe hosted Checkout URL for a paid `plan`; caller does a full nav to it.
export async function createCheckout(plan: Plan): Promise<string> {
  const res = await fetch(`${API_BASE}/api/billing/checkout`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ plan }),
    credentials: "include", // Checkout's client_reference_id is keyed off this cookie
  });

  if (!res.ok) throw await problemError(res, "Checkout failed");
  return (await readData<SessionUrlDto>(res)).url;
}

// Returns a Stripe Customer Portal URL; fails (404/409) when the caller has no Stripe customer yet.
export async function createPortal(): Promise<string> {
  const res = await fetch(`${API_BASE}/api/billing/portal`, {
    method: "POST",
    credentials: "include",
  });

  if (!res.ok) throw await problemError(res, "Portal failed");
  return (await readData<SessionUrlDto>(res)).url;
}
