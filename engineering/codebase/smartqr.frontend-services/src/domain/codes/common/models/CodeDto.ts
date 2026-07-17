import type { Temporal } from "temporal-polyfill";

import type { BarcodeFormat, CodeContent } from "../../content";
import type { CodeRuleDto } from "../../rules";
import type { CodeStyleDto } from "../../style";

/** Represents an issued code — its identity, wire settings, routing rules, style, and typed content. */
export interface CodeDto {
  /** The code's unique id. */
  id: string;

  /** The short slug printed on the code. */
  slug: string;

  /** The resolved short URL the code encodes. */
  shortUrl: string;

  /** The code's display name. */
  name: string;

  /** The symbology the code renders as. */
  barcodeFormat: BarcodeFormat;

  /** Whether the code is active. */
  isActive: boolean;

  /** The lifetime scan count. */
  scanCount: number;

  /** When the code was created. */
  createdAt: Temporal.Instant;

  /** The ordered routing rules. */
  rules: CodeRuleDto[];

  /** The persisted visual style. */
  style: CodeStyleDto;

  /** The persisted typed content — always present. */
  content: CodeContent;
}
