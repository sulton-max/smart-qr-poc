import { Field, TextInput } from "@wow-two-beta/ui/presentation/forms";
import type { VCardContent } from "@/domain/codes/content";
import { type ContentControlsProps, TextAreaField } from "./fields";

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
        <TextInput ring="sm" value={value.phone ?? ""} onChange={(e) => onChange({ ...value, phone: e.target.value || undefined })} />
      </Field>
      <Field label="Email">
        <TextInput ring="sm" value={value.email ?? ""} onChange={(e) => onChange({ ...value, email: e.target.value || undefined })} />
      </Field>
      <Field label="Website">
        <TextInput ring="sm" value={value.url ?? ""} onChange={(e) => onChange({ ...value, url: e.target.value || undefined })} />
      </Field>
      <Field label="Address">
        <TextInput ring="sm" value={value.address ?? ""} onChange={(e) => onChange({ ...value, address: e.target.value || undefined })} />
      </Field>
      <TextAreaField label="Note" value={value.note ?? ""} onChange={(v) => onChange({ ...value, note: v || undefined })} />
    </>
  );
}
