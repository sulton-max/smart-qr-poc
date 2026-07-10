import type { TextContent } from "@/domain/codes/content";
import { type ContentControlsProps, TextAreaField } from "./fields";

/** Renders text content — a single free-text payload. */
export function TextControls({ value, onChange }: ContentControlsProps<TextContent>) {
  return <TextAreaField label="Text" value={value.text} onChange={(text) => onChange({ ...value, text })} />;
}
