import { Field, TextInput } from "@wow-two-beta/ui/presentation/forms";
import type { EmailContent } from "@/domain/codes/content";
import { type ContentControlsProps, TextAreaField } from "./fields";

/** Renders email content — recipient, optional subject, optional (multiline) body. */
export function EmailControls({ value, onChange }: ContentControlsProps<EmailContent>) {
  return (
    <>
      <Field label="To">
        <TextInput
          ring="sm"
          value={value.to}
          placeholder="name@example.com"
          onChange={(e) => onChange({ ...value, to: e.target.value })}
        />
      </Field>
      <Field label="Subject">
        <TextInput ring="sm" value={value.subject ?? ""} onChange={(e) => onChange({ ...value, subject: e.target.value || undefined })} />
      </Field>
      <TextAreaField label="Body" value={value.body ?? ""} onChange={(v) => onChange({ ...value, body: v || undefined })} />
    </>
  );
}
