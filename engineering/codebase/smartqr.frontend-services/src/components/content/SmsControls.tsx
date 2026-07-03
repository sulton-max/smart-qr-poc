import { contentType } from "../../lib/contentTypes";
import { type ContentControlsProps, TextAreaField, TextField } from "./fields";

const [phone, message] = contentType("sms").fields;

/** SMS content — recipient phone plus an optional prefilled (multiline) message. */
export function SmsControls({ values, onChange }: ContentControlsProps) {
  const set = (key: string, v: string) => onChange({ ...values, [key]: v });
  return (
    <>
      <TextField label={phone.label} value={values[phone.key] ?? ""} placeholder={phone.placeholder} onChange={(v) => set(phone.key, v)} />
      <TextAreaField label={message.label} value={values[message.key] ?? ""} placeholder={message.placeholder} onChange={(v) => set(message.key, v)} />
    </>
  );
}
