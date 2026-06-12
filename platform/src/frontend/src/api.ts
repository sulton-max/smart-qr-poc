import type { ApiSuccess, CodeDto, CreateCodeRequest, Me, UpdateCodeRequest } from "./types";

/**
 * Management API base. Empty = same-origin (relative) — the Api serves this SPA in prod, and the
 * Vite dev server proxies `/api` → the Api. Override via VITE_API_BASE for a split deployment.
 */
export const API_BASE: string = import.meta.env.VITE_API_BASE ?? "";

/** Redirect service base — used to build preview short URLs. Override via VITE_REDIRECT_BASE. */
export const REDIRECT_BASE: string = import.meta.env.VITE_REDIRECT_BASE ?? "http://localhost:7022";

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
