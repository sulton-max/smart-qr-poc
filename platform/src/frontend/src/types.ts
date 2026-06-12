// Frontend mirror of the backend contract (SmartQr.Common.Domain enums + Api DTOs).

export const RuleConditionType = {
  Device: "Device",
  Country: "Country",
  Language: "Language",
  TimeOfDay: "TimeOfDay",
} as const;
export type RuleConditionType = (typeof RuleConditionType)[keyof typeof RuleConditionType];

export const BarcodeFormat = {
  QrCode: "QrCode",
  DataMatrix: "DataMatrix",
  Pdf417: "Pdf417",
  Aztec: "Aztec",
  Code128: "Code128",
  Ean13: "Ean13",
  UpcA: "UpcA",
} as const;
export type BarcodeFormat = (typeof BarcodeFormat)[keyof typeof BarcodeFormat];

/** A routing rule row in the builder (client-side `id` for list keys). */
export interface RuleDraft {
  id: string;
  order: number;
  conditionType: RuleConditionType;
  conditionValue: string;
  destination: string;
}

export interface CreateCodeRequest {
  name: string;
  codeType: "Qr" | "Barcode" | "Link";
  barcodeFormat: BarcodeFormat;
  fallbackUrl: string;
  rules: Array<{
    order: number;
    conditionType: RuleConditionType;
    conditionValue: string;
    destination: string;
  }>;
}

/**
 * Body for `PUT /api/codes/{id}` — full replace of the editable fields plus the entire rule set.
 * Mirrors `CreateCodeRequest`; the slug, scan count, and creation time are server-preserved.
 */
export type UpdateCodeRequest = CreateCodeRequest;

/** Body for `PATCH /api/codes/{id}/active` — toggles only `is_active`. */
export interface SetActiveRequest {
  isActive: boolean;
}

export interface CodeDto {
  id: string;
  slug: string;
  shortUrl: string;
  name: string;
  codeType: string;
  barcodeFormat: BarcodeFormat;
  fallbackUrl: string;
  isActive: boolean;
  neverExpires: boolean;
  scanCount: number;
  createdAt: string;
  rules: Array<{
    order: number;
    conditionType: RuleConditionType;
    conditionValue: string | null;
    destination: string;
  }>;
}

/** Backend `ApiResponse<T>.Success` envelope (camelCased). */
export interface ApiSuccess<T> {
  data: T;
}

// Identity
export type UserKind = "Anonymous" | "Guest" | "User";

export interface UserSummary {
  id: string;
  name: string;
  email: string;
}

export interface Me {
  kind: UserKind;
  user: UserSummary | null;
}
