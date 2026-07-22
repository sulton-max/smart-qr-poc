import type { CodeDto, ImageFormat } from "@/domain/codes/common";
import { API_BASE, problemError, readData } from "../common/client";
import type { CodeCreateUpdateApiRequest, CodePreviewApiRequest, CodeSetActiveApiRequest } from "./models";

/** The codes API client — code CRUD, image URLs, and the server-rendered live preview. */
export const codesApiClient = {
  /** Creates a code; the guest owner cookie ties it to this visitor. */
  async create(request: CodeCreateUpdateApiRequest): Promise<CodeDto> {
    const res = await fetch(`${API_BASE}/api/codes`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
      credentials: "include",
    });

    if (!res.ok) throw await problemError(res, "Create failed");
    return readData<CodeDto>(res);
  },

  /** Lists the owner's codes; `q` case-insensitively filters name (server-side `contains`). */
  async list(q?: string): Promise<CodeDto[]> {
    const query = q && q.trim() ? `?q=${encodeURIComponent(q.trim())}` : "";
    const res = await fetch(`${API_BASE}/api/codes${query}`, {
      credentials: "include",
    });

    if (!res.ok) throw await problemError(res, "List failed");
    return readData<CodeDto[]>(res);
  },

  /** Gets one code by id — 404 when missing or owned by someone else. */
  async get(id: string): Promise<CodeDto> {
    const res = await fetch(`${API_BASE}/api/codes/${id}`, {
      credentials: "include",
    });

    if (!res.ok) throw await problemError(res, "Load failed");
    return readData<CodeDto>(res);
  },

  /** Replaces a code in full; slug, scan count, and creation time are server-preserved. */
  async update(id: string, request: CodeCreateUpdateApiRequest): Promise<CodeDto> {
    const res = await fetch(`${API_BASE}/api/codes/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
      credentials: "include",
    });

    if (!res.ok) throw await problemError(res, "Update failed");
    return readData<CodeDto>(res);
  },

  /** Toggles a code's active state. */
  async setActive(id: string, isActive: boolean): Promise<CodeDto> {
    const request: CodeSetActiveApiRequest = { isActive };
    const res = await fetch(`${API_BASE}/api/codes/${id}/active`, {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
      credentials: "include",
    });

    if (!res.ok) throw await problemError(res, "Status change failed");
    return readData<CodeDto>(res);
  },

  /** Hard-deletes a code; cascades its rules. */
  async delete(id: string): Promise<void> {
    const res = await fetch(`${API_BASE}/api/codes/${id}`, {
      method: "DELETE",
      credentials: "include",
    });

    if (!res.ok) throw await problemError(res, "Delete failed");
  },

  /** Builds the download URL for a code's rendered image in `format`. */
  imageUrl(id: string, format: ImageFormat): string {
    return `${API_BASE}/api/codes/${id}/image?format=${format}`;
  },

  /** Renders a live preview as server-emitted SVG markup; pass an `AbortSignal` to cancel superseded requests. */
  async preview(request: CodePreviewApiRequest, signal?: AbortSignal): Promise<string> {
    const res = await fetch(`${API_BASE}/api/codes/preview`, {
      method: "POST",
      headers: { "Content-Type": "application/json", Accept: "image/svg+xml" },
      body: JSON.stringify(request),
      credentials: "include",
      signal,
    });

    if (!res.ok) throw await problemError(res, "Preview failed");
    return res.text();
  },
};
