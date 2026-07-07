import { ContentTypeId, contentType } from "@/domain/codes/content";
import { type ContentControlsProps, FieldRenderer } from "./fields";

const [phone] = contentType(ContentTypeId.Phone).fields;

/** Renders phone content — a single dial-number field. */
export function PhoneControls({ fieldValues, onChange }: ContentControlsProps) {
  return (
    <FieldRenderer field={phone} value={fieldValues[phone.key]} onChange={(v) => onChange({ ...fieldValues, [phone.key]: v })} />
  );
}
