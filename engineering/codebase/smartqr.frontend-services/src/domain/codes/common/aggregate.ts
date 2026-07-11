// Frontend mirror of the backend code contract (SmartQr.Api Codes DTOs) — the shared aggregate that
// pulls the content / style / rules concerns together: the create/update/preview requests and the CodeDto.

import type { Temporal } from "temporal-polyfill";
import type { CodeContent } from "../content/types";
import type { BarcodeFormat } from "../content/BarcodeFormat";
import type { CodeType } from "../content/CodeType";
import type { RuleConditionType } from "../rules/RuleConditionType";
import type { PreviewStyle } from "../style/PreviewStyle";

export interface CreateCodeRequest {
  name: string;
  codeType: CodeType;
  barcodeFormat: BarcodeFormat;
  fallbackUrl: string;
  rules: ReadonlyArray<{
    order: number;
    conditionType: RuleConditionType;
    conditionValue: string;
    destination: string;
  }>;
  // The visual style to persist (server defaults it when omitted; the builder always sends it).
  style: PreviewStyle;
  // The typed content (discriminated on `type`); the builder always sends it. Encoded server-side.
  content: CodeContent;
}

// `PUT /api/codes/{id}` — full replace; slug, scan count, creation time are server-preserved.
export type UpdateCodeRequest = CreateCodeRequest;

export interface SetActiveRequest {
  isActive: boolean;
}

// ── Preview (POST /api/codes/preview → image/svg+xml) ──────────────────────────
// Server-authoritative render for the builder's live preview — same engine that
// produces the downloadable asset, so the preview is byte-for-byte parity.
export interface PreviewRequest {
  // Fallback data when `content` is absent/dynamic — the short link on edit, a sample URL on create.
  value: string;
  codeType: CodeType;
  style: PreviewStyle;
  // Typed content; when static, the server encodes its payload so the preview matches the saved asset.
  content: CodeContent | null;
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
  createdAt: Temporal.Instant;
  rules: ReadonlyArray<{
    order: number;
    conditionType: RuleConditionType;
    conditionValue: string | null;
    destination: string;
  }>;
  // Persisted visual style (enum values come back verbatim/PascalCase — normalize on read).
  style: PreviewStyle;
  // Persisted typed content (discriminated on `type`); null for a legacy/plain code.
  content: CodeContent | null;
}
