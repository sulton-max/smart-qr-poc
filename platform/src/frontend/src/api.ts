import type { ApiSuccess, CodeDto, CreateCodeRequest } from "./types";

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
  });

  if (!res.ok) {
    throw new Error(`Create failed (HTTP ${res.status})`);
  }

  const json = (await res.json()) as ApiSuccess<CodeDto>;
  return json.data;
}

/** URL of the server-rendered code image (vector SVG or raster PNG). */
export function codeImageUrl(id: string, format: "svg" | "png"): string {
  return `${API_BASE}/api/codes/${id}/image?format=${format}`;
}
