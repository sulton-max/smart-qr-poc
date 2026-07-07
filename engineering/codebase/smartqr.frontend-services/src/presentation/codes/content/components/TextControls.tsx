import { ContentTypeId, contentType } from "@/domain/codes/content";
import { type ContentControlsProps, FieldRenderer } from "./fields";

const [text] = contentType(ContentTypeId.Text).fields;

/** Renders text content — a single free-text textarea. */
export function TextControls({ fieldValues, onChange }: ContentControlsProps) {
  return (
    <FieldRenderer field={text} value={fieldValues[text.key]} onChange={(v) => onChange({ ...fieldValues, [text.key]: v })} />
  );
}
