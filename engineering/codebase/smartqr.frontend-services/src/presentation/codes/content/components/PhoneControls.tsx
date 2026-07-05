import { contentType } from "@/domain/codes/content";
import { type ContentControlsProps, TextField } from "./fields";

const [phone] = contentType("phone").fields;

/** Phone content — a single dial-number field. */
export function PhoneControls({ values, onChange }: ContentControlsProps) {
  return (
    <TextField
      label={phone.label}
      value={values[phone.key] ?? ""}
      placeholder={phone.placeholder}
      onChange={(v) => onChange({ ...values, [phone.key]: v })}
    />
  );
}
