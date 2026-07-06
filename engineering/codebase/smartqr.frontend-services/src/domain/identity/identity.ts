// Identity — mirrors the backend `SmartQr.Api` identity DTOs.
import type { UserKind } from "./enums/UserKind";

export interface UserSummary {
  id: string;
  name: string;
  email: string;
}

export interface Me {
  kind: UserKind;
  user: UserSummary | null;
}
