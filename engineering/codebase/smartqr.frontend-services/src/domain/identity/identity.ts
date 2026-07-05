// Identity — mirrors the backend `SmartQr.Api` identity DTOs.

// camelCase wire (backend `UserKind` enum).
export type UserKind = "anonymous" | "guest" | "user";

export interface UserSummary {
  id: string;
  name: string;
  email: string;
}

export interface Me {
  kind: UserKind;
  user: UserSummary | null;
}
