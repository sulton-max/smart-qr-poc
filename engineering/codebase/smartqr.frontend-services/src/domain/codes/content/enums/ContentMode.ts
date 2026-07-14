/** Defines how a code carries its payload — baked directly (static) vs via the forwarder short link (dynamic). */
export const ContentMode = {
  /** Refers to a code that bakes the payload directly into the symbol (no redirect). */
  Static: "static",

  /** Refers to a code that carries the forwarder short link, resolved at scan time. */
  Dynamic: "dynamic",
} as const;

export type ContentMode = (typeof ContentMode)[keyof typeof ContentMode];
