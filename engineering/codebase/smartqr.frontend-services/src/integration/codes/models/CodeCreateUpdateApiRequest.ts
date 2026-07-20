import type { BarcodeFormat, ContentMode, ContentType, CodeRuleDto, CodeStyleDto } from "@/domain/codes";

/** Defines the create/update code request body — shared for POST create and PUT replace (the id rides the URL). */
export interface CodeCreateUpdateApiRequest {
  /** The code's display name. */
  name: string;

  /** The symbology the code renders as. */
  barcodeFormat: BarcodeFormat;

  /** How the symbol resolves. Sent on create only — an edit can never change what the symbol bakes. */
  mode?: ContentMode;

  /** The kind of content every rule carries. */
  contentType: ContentType;

  /** The routing rules, each carrying the content it serves. At least one is required. */
  rules: CodeRuleDto[];

  /** The visual style to persist. */
  style: CodeStyleDto;
}
