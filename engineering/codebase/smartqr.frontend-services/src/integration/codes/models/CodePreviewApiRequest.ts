import type { CodeContent, CodeStyleDto, CodeType } from "@/domain/codes";

/** Defines the live-preview request body — renders a code without persisting it. */
export interface CodePreviewApiRequest {
  /** The fallback data when content is dynamic or absent. */
  value: string;

  /** The coarse render kind. */
  codeType: CodeType;

  /** The visual style to render. */
  style: CodeStyleDto;

  /** The typed content to bake into the preview, when present. */
  content?: CodeContent;
}
