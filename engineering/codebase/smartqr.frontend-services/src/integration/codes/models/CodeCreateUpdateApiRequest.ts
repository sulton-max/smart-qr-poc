import type { BarcodeFormat, ContentMode, ContentType, CodeRuleDto, CodeStyleDto } from "@/domain/codes";

/**
 * Defines the create/update code request body — shared for POST create and PUT replace (the id rides the URL).
 * Every member is always present; `toUpdateCodeRequest` drops `mode` at the wire, where the contract omits it.
 */
export interface CodeCreateUpdateApiRequest {
  /** The code's display name. */
  name: string;

  /** The symbology the code renders as. */
  barcodeFormat: BarcodeFormat;

  /** How the symbol resolves. Fixed at create — an edit can never change what the symbol bakes, so the update body omits it. */
  mode: ContentMode;

  /** The kind of content every rule carries. */
  contentType: ContentType;

  /** The routing rules, each carrying the content it serves. At least one is required. */
  rules: CodeRuleDto[];

  /** The visual style to persist. */
  style: CodeStyleDto;
}

/** Defines the update code request body — the create body without `mode`, which is fixed at create (CM3). */
export type CodeUpdateApiRequest = Omit<CodeCreateUpdateApiRequest, "mode">;
