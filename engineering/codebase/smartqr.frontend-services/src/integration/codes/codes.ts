import type { CodeDto, CreateCodeRequest, ImageFormat, PreviewRequest, UpdateCodeRequest } from "@/domain/codes/core";
import { API_BASE, problemError, readData } from "../common/client";

export async function createCode(request: CreateCodeRequest): Promise<CodeDto> {
  const res = await fetch(`${API_BASE}/api/codes`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
    credentials: "include", // guest owner cookie ties the code to this visitor
  });

  if (!res.ok) throw await problemError(res, "Create failed");
  return readData<CodeDto>(res);
}

// `q` case-insensitively filters on name or fallback URL (server-side `contains`).
export async function listCodes(q?: string): Promise<CodeDto[]> {
  const query = q && q.trim() ? `?q=${encodeURIComponent(q.trim())}` : "";
  const res = await fetch(`${API_BASE}/api/codes${query}`, {
    credentials: "include", // owner-scoped via cookie
  });

  if (!res.ok) throw await problemError(res, "List failed");
  return readData<CodeDto[]>(res);
}

// 404 when the code is missing or owned by someone else.
export async function getCode(id: string): Promise<CodeDto> {
  const res = await fetch(`${API_BASE}/api/codes/${id}`, {
    credentials: "include",
  });

  if (!res.ok) throw await problemError(res, "Load failed");
  return readData<CodeDto>(res);
}

// Full replace; slug, scan count, and creation time are server-preserved.
export async function updateCode(id: string, request: UpdateCodeRequest): Promise<CodeDto> {
  const res = await fetch(`${API_BASE}/api/codes/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
    credentials: "include",
  });

  if (!res.ok) throw await problemError(res, "Update failed");
  return readData<CodeDto>(res);
}

export async function setCodeActive(id: string, isActive: boolean): Promise<CodeDto> {
  const res = await fetch(`${API_BASE}/api/codes/${id}/active`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ isActive }),
    credentials: "include",
  });

  if (!res.ok) throw await problemError(res, "Status change failed");
  return readData<CodeDto>(res);
}

// Hard-delete; cascades rules.
export async function deleteCode(id: string): Promise<void> {
  const res = await fetch(`${API_BASE}/api/codes/${id}`, {
    method: "DELETE",
    credentials: "include",
  });

  if (!res.ok) throw await problemError(res, "Delete failed");
}

export function codeImageUrl(id: string, format: ImageFormat): string {
  return `${API_BASE}/api/codes/${id}/image?format=${format}`;
}

// Server-authoritative live preview: renders the code with the builder's current
// value + style and returns raw SVG markup (Content-Type: image/svg+xml — NOT the
// JSON envelope). Used by the builder so the preview matches the downloadable asset.
// Pass an AbortSignal so superseded (debounced) requests can be cancelled.
export async function previewCode(
  request: PreviewRequest,
  signal?: AbortSignal,
): Promise<string> {
  const res = await fetch(`${API_BASE}/api/codes/preview`, {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "image/svg+xml" },
    body: JSON.stringify(request),
    credentials: "include",
    signal,
  });

  if (!res.ok) throw await problemError(res, "Preview failed");
  return res.text();
}
