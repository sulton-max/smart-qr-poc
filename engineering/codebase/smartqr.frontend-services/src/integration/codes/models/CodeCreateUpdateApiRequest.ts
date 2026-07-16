import type { BarcodeFormat, CodeContent, CodeRuleDto, CodeStyleDto, CodeType } from "@/domain/codes";

/** Defines the create/update code request body — shared for POST create and PUT replace (the id rides the URL). */
export interface CodeCreateUpdateApiRequest {
  /** The code's display name. */
  name: string;

  /** The coarse render kind. */
  codeType: CodeType;

  /** The symbology the code renders as. */
  barcodeFormat: BarcodeFormat;

  /** The ordered routing rules (first match wins; a trailing `Default` rule is the catch-all). */
  rules: CodeRuleDto[];

  /** The visual style to persist. */
  style: CodeStyleDto;

  /** The typed content the code carries. */
  content: CodeContent;
}
