import { Field, TextInput } from "@wow-two-beta/ui/presentation/forms";
import type { SmsContent } from "@/domain/codes/content";
import { type ContentControlsProps, TextAreaField } from "./fields";

/** Renders SMS content — recipient phone plus an optional prefilled message. */
export function SmsControls({ value, onChange }: ContentControlsProps<SmsContent>) {
  return (
    <>
      <Field label="Phone">
        <TextInput ring="sm" value={value.phone} onChange={(e) => onChange({ ...value, phone: e.target.value })} />
      </Field>
      <TextAreaField label="Message" value={value.message ?? ""} onChange={(v) => onChange({ ...value, message: v || undefined })} />
    </>
  );
}
