import { ContentTypeId, contentType } from "@/domain/codes/content";
import { type ContentControlsProps, FieldRenderer } from "./fields";

// firstName / lastName / org / title / phone / email / url / address are single-line; `note` is a textarea.
const fields = contentType(ContentTypeId.VCard).fields;

/** Renders contact-card (vCard) content — eight single-line fields plus a multiline note. */
export function VCardControls({ fieldValues, onChange }: ContentControlsProps) {
  const setValue = (key: string, v: string) => onChange({ ...fieldValues, [key]: v });

  return (
    <>
      {fields.map((f) => (
        <FieldRenderer key={f.key} field={f} value={fieldValues[f.key]} onChange={(v) => setValue(f.key, v)} />
      ))}
    </>
  );
}
