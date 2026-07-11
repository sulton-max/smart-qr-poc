import { EmailInput, Field, TextInput, TextAreaInput } from "@wow-two-beta/ui/presentation/forms";
import type { EmailContent } from "@/domain/codes/content";
import type { ContentControlsProps } from "./fields";

/** Renders email content — recipient, optional subject, optional (multiline) body. */
export function EmailControls({ value, onChange }: ContentControlsProps<EmailContent>) {
  return (
    <>
      <Field label="To">
        <EmailInput
          ring="sm"
          value={value.to}
          placeholder="name@example.com"
          onChange={(e) => onChange({ ...value, to: e.target.value })}
        />
      </Field>
      <Field label="Subject">
        <TextInput ring="sm" value={value.subject ?? ""} onChange={(e) => onChange({ ...value, subject: e.target.value || undefined })} />
      </Field>
      <Field label="Body">
        <TextAreaInput ring="sm" rows={3} value={value.body ?? ""} onChange={(e) => onChange({ ...value, body: e.target.value || undefined })} />
      </Field>
    </>
  );
}
