import type { Temporal } from "temporal-polyfill";

import type { BarcodeFormat, ContentMode, ContentType } from "../../content";
import type { CodeRuleDto } from "../../rules";
import type { CodeStyleDto } from "../../style";

/** Represents an issued code — its identity, wire settings, style, and the rules carrying its content. */
export interface CodeDto {
  /** The code's unique id. */
  id: string;

  /** The short slug printed on a dynamic code; absent on a static code. */
  slug?: string;

  /** The short URL a dynamic code encodes; absent on a static code. */
  shortUrl?: string;

  /** The code's display name. */
  name: string;

  /** The symbology the code renders as. */
  barcodeFormat: BarcodeFormat;

  /** How the symbol resolves — baked payload (static) or short link (dynamic). Fixed at create. */
  mode: ContentMode;

  /** The kind of content every rule of this code carries. */
  contentType: ContentType;

  /** Whether the code is active. */
  isActive: boolean;

  /** The lifetime scan count; always zero on a static code, which never reaches the server. */
  scanCount: number;

  /** When the code was created. */
  createdAt: Temporal.Instant;

  /** The routing rules, each carrying the content it serves. */
  rules: CodeRuleDto[];

  /** The persisted visual style. */
  style: CodeStyleDto;
}
