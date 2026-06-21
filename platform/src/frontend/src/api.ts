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

// Empty = same-origin; serves the SPA in prod. Override via VITE_API_BASE for split deployment.
export const API_BASE: string = import.meta.env.VITE_API_BASE ?? "";

export const REDIRECT_BASE: string = import.meta.env.VITE_REDIRECT_BASE ?? "http://localhost:7022";

// Public; also the backend's token audience. Empty leaves sign-in inert.
export const GOOGLE_CLIENT_ID: string = import.meta.env.VITE_GOOGLE_CLIENT_ID ?? "";

export async function createCode(request: CreateCodeRequest): Promise<CodeDto> {
  const res = await fetch(`${API_BASE}/api/codes`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
    credentials: "include", // guest owner cookie ties the code to this visitor
  });

  if (!res.ok) {
    throw new Error(`Create failed (HTTP ${res.status})`);
  }

  const json = (await res.json()) as ApiSuccess<CodeDto>;
  return json.data;
}

// `q` case-insensitively filters on name or fallback URL (server-side `contains`).
export async function listCodes(q?: string): Promise<CodeDto[]> {
  const query = q && q.trim() ? `?q=${encodeURIComponent(q.trim())}` : "";
  const res = await fetch(`${API_BASE}/api/codes${query}`, {
    credentials: "include", // owner-scoped via cookie
  });

  if (!res.ok) {
    throw new Error(`List failed (HTTP ${res.status})`);
  }

  const json = (await res.json()) as ApiSuccess<CodeDto[]>;
  return json.data;
}

// 404 when the code is missing or owned by someone else.
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

// Full replace; slug, scan count, and creation time are server-preserved.
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

// Hard-delete; cascades rules.
export async function deleteCode(id: string): Promise<void> {
  const res = await fetch(`${API_BASE}/api/codes/${id}`, {
    method: "DELETE",
    credentials: "include",
  });

  if (!res.ok) {
    throw new Error(`Delete failed (HTTP ${res.status})`);
  }
}

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

// Exchanges a Google ID token for a session; backend upserts the user and sets the auth cookie.
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

// Drops the auth cookie server-side.
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

// Owner-scoped; a guest with no subscription row resolves to Free/active.
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

// Returns a Stripe hosted Checkout URL for a paid `plan`; caller does a full nav to it.
export async function createCheckout(plan: Plan): Promise<string> {
  const res = await fetch(`${API_BASE}/api/billing/checkout`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ plan }),
    credentials: "include", // Checkout's client_reference_id is keyed off this cookie
  });

  if (!res.ok) {
    throw new Error(`Checkout failed (HTTP ${res.status})`);
  }

  const json = (await res.json()) as ApiSuccess<SessionUrlDto>;
  return json.data.url;
}

// Returns a Stripe Customer Portal URL; fails (404/409) when the caller has no Stripe customer yet.
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
