// Content operations — map the builder's flat form values to/from the typed `CodeContent` the wire carries.
// Payload ENCODING lives on the backend: static types bake their payload from these fields server-side, dynamic
// types (url / mobileApp) resolve the redirect short link. No local payload encoders (the frontend↔backend drift
// risk this rewire removes). The per-type field registry lives in `registry.ts`.

import type { CodeContent } from "./types";
import { ContentTypeId, contentType, type FieldValues } from "./registry";

/**
 * Builds the typed `CodeContent` the wire carries from the builder's collected field values. Required fields are
 * always sent (may be empty); optional fields are omitted when blank so the backend sees them as absent (null) — this
 * is what makes mobileApp's "at least one link" and its device-rule derivation correct. `wifi.hidden` maps to a bool.
 */
export function buildContent(id: ContentTypeId, values: FieldValues): CodeContent {
  const out: Record<string, unknown> = { type: id };

  for (const field of contentType(id).fields) {
    if (id === ContentTypeId.Wifi && field.key === "hidden") {
      out.hidden = values.hidden === "true";
      continue;
    }

    const value = values[field.key] ?? "";
    if (field.required) out[field.key] = value;
    else if (value.trim() !== "") out[field.key] = value;
  }

  // The mobile-app fallback picker isn't a registry field — carry the chosen store key through when set.
  if (id === ContentTypeId.MobileApp && values.fallback) out.fallback = values.fallback;

  // Structural cast via `unknown`: `out` is assembled dynamically, so it can't be narrowed to a single
  // union member statically — the per-type field loop guarantees the right shape at runtime.
  return out as unknown as CodeContent;
}

/** Projects a persisted `CodeContent` back to the builder's flat field values (for the edit round-trip). Inverse of `buildContent`. */
export function contentToValues(content: CodeContent): FieldValues {
  const values: FieldValues = {};

  for (const [key, value] of Object.entries(content)) {
    if (key === "type") continue;
    if (typeof value === "boolean") values[key] = value ? "true" : "false";
    else if (value != null) values[key] = String(value);
  }

  return values;
}

/** A code resolves through its redirect short link (dynamic) rather than a baked payload — true for url / mobileApp / legacy-null content. */
export function isDynamicContent(content: CodeContent | null): boolean {
  return content == null || contentType(content.type).mode === "dynamic";
}
