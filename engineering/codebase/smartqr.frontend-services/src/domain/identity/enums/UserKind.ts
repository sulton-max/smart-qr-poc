/** Defines the current user's kind (mirrors backend `UserKind`; camelCase wire). */
export const UserKind = {
  /** Refers to an unidentified visitor (no session). */
  Anonymous: "anonymous",
  /** Refers to a provisional guest session (pre-sign-in). */
  Guest: "guest",
  /** Refers to a signed-in user. */
  User: "user",
} as const;

export type UserKind = (typeof UserKind)[keyof typeof UserKind];
