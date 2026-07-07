import { ContentTypeId, contentType } from "@/domain/codes/content";
import { type ContentControlsProps, FieldRenderer } from "./fields";

// phone is a tel; message is a textarea (kinds come from the registry).
const fields = contentType(ContentTypeId.Sms).fields;

/** Renders SMS content — recipient phone plus an optional prefilled (multiline) message. */
export function SmsControls({ fieldValues, onChange }: ContentControlsProps) {
  const setValue = (key: string, v: string) => onChange({ ...fieldValues, [key]: v });

  return (
    <>
      {fields.map((f) => (
        <FieldRenderer key={f.key} field={f} value={fieldValues[f.key]} onChange={(v) => setValue(f.key, v)} />
      ))}
    </>
  );
}
