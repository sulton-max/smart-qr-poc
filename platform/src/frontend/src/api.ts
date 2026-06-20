import type {
  ApiSuccess,
  BillingStatus,
  CodeDto,
  CreateCodeRequest,
  Me,
  Plan,
  SessionUrlDto,
  UpdateCodeRequest,
} from "./types";

/**
 * Management API base. Empty = same-origin (relative) — the Api serves this SPA in prod, and the
 * Vite dev server proxies `/api` → the Api. Override via VITE_API_BASE for a split deployment.
 */
export const API_BASE: string = import.meta.env.VITE_API_BASE ?? "";

/** Redirect service base — used to build preview short URLs. Override via VITE_REDIRECT_BASE. */
export const REDIRECT_BASE: string = import.meta.env.VITE_REDIRECT_BASE ?? "http://localhost:7022";

/**
 * Google OAuth Web client id — public, also serves as the backend's token audience. Set via
 * VITE_GOOGLE_CLIENT_ID; empty when unconfigured (sign-in button stays inert).
 */
export const GOOGLE_CLIENT_ID: string = import.meta.env.VITE_GOOGLE_CLIENT_ID ?? "";

export async function createCode(request: CreateCodeRequest): Promise<CodeDto> {
  const res = await fetch(`${API_BASE}/api/codes`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
    credentials: "include", // carry the guest owner cookie so the code is tied to this visitor
  });

  if (!res.ok) {
    throw new Error(`Create failed (HTTP ${res.status})`);
  }

  const json = (await res.json()) as ApiSuccess<CodeDto>;
  return json.data;
}

/**
 * Lists the caller's codes, newest first. Pass `q` to case-insensitively filter on name or
 * fallback URL (server-side `contains`).
 */
export async function listCodes(q?: string): Promise<CodeDto[]> {
  const query = q && q.trim() ? `?q=${encodeURIComponent(q.trim())}` : "";
  const res = await fetch(`${API_BASE}/api/codes${query}`, {
    credentials: "include", // owner-scoped — the cookie identifies whose codes to return
  });

  if (!res.ok) {
    throw new Error(`List failed (HTTP ${res.status})`);
  }

  const json = (await res.json()) as ApiSuccess<CodeDto[]>;
  return json.data;
}

/** Fetches a single owned code by id. 404 when the code is missing or owned by someone else. */
export async function getCode(id: string): Promise<CodeDto> {
  const res = await fetch(`${API_BASE}/api/codes/${id}`, {
    credentials: "include",
  });

  if (!res.ok) {
    throw new Error(`Load failed (HTTP ${res.status})`);
  }

  const json = (await res.json()) as ApiSuccess<CodeDto>;
  return json.data;
}

/**
 * Replaces the editable fields and the whole rule set of an owned code. The slug, scan count,
 * and creation time are preserved server-side.
 */
export async function updateCode(id: string, request: UpdateCodeRequest): Promise<CodeDto> {
  const res = await fetch(`${API_BASE}/api/codes/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
    credentials: "include",
  });

  if (!res.ok) {
    throw new Error(`Update failed (HTTP ${res.status})`);
  }

  const json = (await res.json()) as ApiSuccess<CodeDto>;
  return json.data;
}

/** Enables or disables an owned code (toggles `is_active` only). */
export async function setCodeActive(id: string, isActive: boolean): Promise<CodeDto> {
  const res = await fetch(`${API_BASE}/api/codes/${id}/active`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ isActive }),
    credentials: "include",
  });

  if (!res.ok) {
    throw new Error(`Status change failed (HTTP ${res.status})`);
  }

  const json = (await res.json()) as ApiSuccess<CodeDto>;
  return json.data;
}

/** Hard-deletes an owned code (cascades its rules). Resolves on the 204 No Content. */
export async function deleteCode(id: string): Promise<void> {
  const res = await fetch(`${API_BASE}/api/codes/${id}`, {
    method: "DELETE",
    credentials: "include",
  });

  if (!res.ok) {
    throw new Error(`Delete failed (HTTP ${res.status})`);
  }
}

/** URL of the server-rendered code image (vector SVG or raster PNG). */
export function codeImageUrl(id: string, format: "svg" | "png"): string {
  return `${API_BASE}/api/codes/${id}/image?format=${format}`;
}

export async function getMe(): Promise<Me> {
  const res = await fetch(`${API_BASE}/api/identity/me`, {
    credentials: "include",
  });

  if (!res.ok) {
    throw new Error(`Identity check failed (HTTP ${res.status})`);
  }

  const json = (await res.json()) as ApiSuccess<Me>;
  return json.data;
}

export async function createGuest(): Promise<Me> {
  const res = await fetch(`${API_BASE}/api/identity/guest`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
  });

  if (!res.ok) {
    throw new Error(`Guest creation failed (HTTP ${res.status})`);
  }

  const json = (await res.json()) as ApiSuccess<Me>;
  return json.data;
}

/**
 * Exchanges a Google ID token (credential from the Sign-in button) for an authenticated session.
 * The backend verifies the token against GOOGLE_CLIENT_ID, upserts the user, and sets the auth
 * cookie. Resolves to the signed-in `Me`.
 */
export async function signInWithGoogle(idToken: string): Promise<Me> {
  const res = await fetch(`${API_BASE}/api/auth/google`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ idToken }),
    credentials: "include", // server sets the auth cookie on this response
  });

  if (!res.ok) {
    throw new Error(`Sign-in failed (HTTP ${res.status})`);
  }

  return ((await res.json()) as ApiSuccess<Me>).data;
}

/** Clears the authenticated session server-side (drops the auth cookie). Resolves on success. */
export async function logout(): Promise<void> {
  const res = await fetch(`${API_BASE}/api/auth/logout`, {
    method: "POST",
    credentials: "include",
  });

  if (!res.ok) {
    throw new Error(`Logout failed (HTTP ${res.status})`);
  }
}

// ── Billing ──────────────────────────────────────────────────────────────────

/**
 * Current plan, raw subscription status, code cap, and live usage for the caller.
 * Owner-scoped via the guest cookie; a guest with no subscription row resolves to Free/active.
 */
export async function getBilling(): Promise<BillingStatus> {
  const res = await fetch(`${API_BASE}/api/billing/me`, {
    credentials: "include",
  });

  if (!res.ok) {
    throw new Error(`Billing status failed (HTTP ${res.status})`);
  }

  const json = (await res.json()) as ApiSuccess<BillingStatus>;
  return json.data;
}

/**
 * Opens a Stripe Hosted Checkout session for `plan` (a paid plan — the backend rejects `Free`) and
 * returns the hosted URL. The caller redirects the browser to it (`window.location.href = url`).
 */
export async function createCheckout(plan: Plan): Promise<string> {
  const res = await fetch(`${API_BASE}/api/billing/checkout`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ plan }),
    credentials: "include", // owner-scoped — Checkout's client_reference_id is keyed off this cookie
  });

  if (!res.ok) {
    throw new Error(`Checkout failed (HTTP ${res.status})`);
  }

  const json = (await res.json()) as ApiSuccess<SessionUrlDto>;
  return json.data.url;
}

/**
 * Opens a Stripe Customer Portal session for the caller's stored Stripe customer and returns the
 * hosted URL to redirect to. Body-less; fails (404/409) when the caller has no Stripe customer yet.
 */
export async function createPortal(): Promise<string> {
  const res = await fetch(`${API_BASE}/api/billing/portal`, {
    method: "POST",
    credentials: "include",
  });

  if (!res.ok) {
    throw new Error(`Portal failed (HTTP ${res.status})`);
  }

  const json = (await res.json()) as ApiSuccess<SessionUrlDto>;
  return json.data.url;
}
