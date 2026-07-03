import { contentType } from "../../lib/contentTypes";
import { type ContentControlsProps, TextAreaField, TextField } from "./fields";

// firstName / lastName / org / title / phone / email / url / address are single-line; `note` is a textarea.
const fields = contentType("vcard").fields;

/** Contact-card (vCard) content — eight single-line fields plus a multiline note. */
export function VCardControls({ values, onChange }: ContentControlsProps) {
  const set = (key: string, v: string) => onChange({ ...values, [key]: v });
  return (
    <>
      {fields.map((f) =>
        f.kind === "textarea" ? (
          <TextAreaField
            key={f.key}
            label={f.label}
            value={values[f.key] ?? ""}
            placeholder={f.placeholder}
            onChange={(v) => set(f.key, v)}
          />
        ) : (
          <TextField
            key={f.key}
            label={f.label}
            value={values[f.key] ?? ""}
            placeholder={f.placeholder}
            onChange={(v) => set(f.key, v)}
          />
        ),
      )}
    </>
  );
}
