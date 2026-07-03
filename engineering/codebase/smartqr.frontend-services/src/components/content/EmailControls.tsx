import { contentType } from "../../lib/contentTypes";
import { type ContentControlsProps, TextAreaField, TextField } from "./fields";

const [to, subject, body] = contentType("email").fields;

/** Email content — recipient, optional subject, optional (multiline) body. */
export function EmailControls({ values, onChange }: ContentControlsProps) {
  const set = (key: string, v: string) => onChange({ ...values, [key]: v });
  return (
    <>
      <TextField label={to.label} value={values[to.key] ?? ""} placeholder={to.placeholder} onChange={(v) => set(to.key, v)} />
      <TextField label={subject.label} value={values[subject.key] ?? ""} placeholder={subject.placeholder} onChange={(v) => set(subject.key, v)} />
      <TextAreaField label={body.label} value={values[body.key] ?? ""} placeholder={body.placeholder} onChange={(v) => set(body.key, v)} />
    </>
  );
}
