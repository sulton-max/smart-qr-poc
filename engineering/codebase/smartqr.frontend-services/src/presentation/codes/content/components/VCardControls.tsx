import { EmailInput, Field, TelInput, TextInput, TextareaInput, UrlInput } from "@wow-two-beta/ui/presentation/forms";
import type { VCardContent } from "@/domain/codes/content";
import type { ContentControlsProps } from "./fields";

/** Renders contact-card (vCard) content — only the first name is required. */
export function VCardControls({ value, onChange }: ContentControlsProps<VCardContent>) {
  return (
    <>
      <Field label="First name">
        <TextInput ring="sm" value={value.firstName} onChange={(e) => onChange({ ...value, firstName: e.target.value })} />
      </Field>
      <Field label="Last name">
        <TextInput ring="sm" value={value.lastName ?? ""} onChange={(e) => onChange({ ...value, lastName: e.target.value || undefined })} />
      </Field>
      <Field label="Company">
        <TextInput ring="sm" value={value.org ?? ""} onChange={(e) => onChange({ ...value, org: e.target.value || undefined })} />
      </Field>
      <Field label="Title">
        <TextInput ring="sm" value={value.title ?? ""} onChange={(e) => onChange({ ...value, title: e.target.value || undefined })} />
      </Field>
      <Field label="Phone">
        <TelInput ring="sm" value={value.phone ?? ""} onChange={(e) => onChange({ ...value, phone: e.target.value || undefined })} />
      </Field>
      <Field label="Email">
        <EmailInput ring="sm" value={value.email ?? ""} onChange={(e) => onChange({ ...value, email: e.target.value || undefined })} />
      </Field>
      <Field label="Website">
        <UrlInput ring="sm" value={value.url ?? ""} onChange={(e) => onChange({ ...value, url: e.target.value || undefined })} />
      </Field>
      <Field label="Address">
        <TextInput ring="sm" value={value.address ?? ""} onChange={(e) => onChange({ ...value, address: e.target.value || undefined })} />
      </Field>
      <Field label="Note">
        <TextareaInput ring="sm" rows={3} value={value.note ?? ""} onChange={(e) => onChange({ ...value, note: e.target.value || undefined })} />
      </Field>
    </>
  );
}
