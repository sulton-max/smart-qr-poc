import { Field, TelInput, TextAreaInput } from "@wow-two-beta/ui/presentation/forms";
import type { SmsContent } from "@/domain/codes/content";
import type { ContentControlsProps } from "./fields";

/** Renders SMS content — recipient phone plus an optional prefilled message. */
export function SmsControls({ value, onChange }: ContentControlsProps<SmsContent>) {
  return (
    <>
      <Field label="Phone">
        <TelInput ring="sm" value={value.phone} onChange={(e) => onChange({ ...value, phone: e.target.value })} />
      </Field>
      <Field label="Message">
        <TextAreaInput ring="sm" rows={3} value={value.message ?? ""} onChange={(e) => onChange({ ...value, message: e.target.value || undefined })} />
      </Field>
    </>
  );
}
