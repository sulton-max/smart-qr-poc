// Frontend mirror of the backend code contract (SmartQr.Api Codes DTOs + style/preview shapes).
// Enums live one-per-file under `./enums`; the typed content union lives in `../content/types`.

import type { Temporal } from "@js-temporal/polyfill";
import type { CodeContent } from "../content/types";
import type { Gradient } from "@wow-two-beta/ui/domain/color";
import type { BarcodeFormat } from "./enums/BarcodeFormat";
import type { CodeType } from "./enums/CodeType";
import type { EccLevel } from "./enums/EccLevel";
import type { FinderShape } from "./enums/FinderShape";
import type { ModuleShape } from "./enums/ModuleShape";
import type { RuleConditionType } from "./enums/RuleConditionType";

// Builder row; `id` is client-side for list keys.
export interface RuleDraft {
  id: string;
  order: number;
  conditionType: RuleConditionType;
  conditionValue: string;
  destination: string;
}

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

// Optional center logo overlay for the preview render.
export interface PreviewLogo {
  // Data URL (e.g. `data:image/png;base64,…`) of the logo image.
  dataUrl: string;
  // Logo edge as a fraction of the code's width (0–1).
  sizeRatio: number;
}

// Optional center emoji overlay for the preview render (no file upload).
export interface PreviewEmoji {
  char: string;
  sizeRatio: number; // fraction of the code's width (0–1)
}

// Visual style sent with a preview request. Field names match the pinned wire
// contract (camelCase), not the internal C# `CodeRenderOptions` shape.
export interface PreviewStyle {
  foregroundColor: string; // #RRGGBB
  backgroundColor: string; // #RRGGBB
  transparentBackground: boolean;
  eccLevel: EccLevel;
  quietZoneModules: number;
  logo: PreviewLogo | null;
  moduleShape: ModuleShape; // default "square"
  finderShape: FinderShape; // outer eye frame; default "square"
  finderDotShape: FinderShape; // inner eye pupil; default "square"
  gradient: Gradient | null; // foreground gradient; null = solid foregroundColor
  emoji: PreviewEmoji | null; // center emoji overlay; null = none
}

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
