import { ApiError, parseJson } from "@wow-two-beta/ui/http";
import type { ApiResponse, ProblemDetails } from "@wow-two-beta/ui/http";

// Re-export the shared error type so consumers import it from the integration surface, not the SDK directly.
export { ApiError };

// Empty = same-origin; serves the SPA in prod. Override via VITE_API_BASE for split deployment.
export const API_BASE: string = import.meta.env.VITE_API_BASE ?? "";

export const REDIRECT_BASE: string = import.meta.env.VITE_REDIRECT_BASE ?? "http://localhost:7022";

// Public; also the backend's token audience. Empty leaves sign-in inert.
export const GOOGLE_CLIENT_ID: string = import.meta.env.VITE_GOOGLE_CLIENT_ID ?? "";

// Reads a `{ data }` success envelope, parsing through the Temporal reviver so ISO date fields
// (e.g. `createdAt`) arrive as `Temporal.*`, then unwraps the payload.
export async function readData<T>(res: Response): Promise<T> {
  const envelope = parseJson<ApiResponse<T>>(await res.text());
  return envelope.data;
}

// Picks a human message from a problem body: our validation `errors[]` shape ([{ property, message,
// code }]) first, then ASP.NET's ModelState object shape, then `detail`/`title`, falling back to the
// status — so the UI shows "Add at least one link — App Store, …" instead of a bare "HTTP 400".
function problemMessage(problem: ProblemDetails | null, fallback: string, status: number): string {
  const errs = problem?.errors as Array<{ message?: string }> | Record<string, string[]> | undefined;
  if (Array.isArray(errs)) {
    const msgs = errs.map((e) => e?.message).filter((m): m is string => Boolean(m));
    if (msgs.length) return msgs.join(" ");
  } else if (errs && typeof errs === "object") {
    const msgs = Object.values(errs).flat().filter((m): m is string => Boolean(m));
    if (msgs.length) return msgs.join(" ");
  }
  return problem?.detail ?? problem?.title ?? `${fallback} (HTTP ${status})`;
}

// Builds a typed ApiError from a non-2xx response — carries the status + parsed ProblemDetails,
// with a UI-ready message. Callers `throw await problemError(res, "…")`.
export async function problemError(res: Response, fallback: string): Promise<ApiError> {
  let problem: ProblemDetails | null = null;
  try {
    problem = parseJson<ProblemDetails>(await res.text());
  } catch {
    problem = null;
  }
  return new ApiError(res.status, problem, problemMessage(problem, fallback, res.status));
}
