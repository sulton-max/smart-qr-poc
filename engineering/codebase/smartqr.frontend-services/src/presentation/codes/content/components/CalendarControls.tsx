import { Field, TextInput } from "@wow-two-beta/ui/presentation/forms";
import type { CalendarContent } from "@/domain/codes/content";
import { type ContentControlsProps, DateTimeField, TextAreaField } from "./fields";

/** Renders calendar-event content — title + start required, everything else optional. */
export function CalendarControls({ value, onChange }: ContentControlsProps<CalendarContent>) {
  return (
    <>
      <Field label="Title">
        <TextInput ring="sm" value={value.title} onChange={(e) => onChange({ ...value, title: e.target.value })} />
      </Field>
      <DateTimeField label="Starts" value={value.start} onChange={(start) => onChange({ ...value, start })} />
      <DateTimeField label="Ends" value={value.end ?? ""} onChange={(v) => onChange({ ...value, end: v || undefined })} />
      <Field label="Location">
        <TextInput ring="sm" value={value.location ?? ""} onChange={(e) => onChange({ ...value, location: e.target.value || undefined })} />
      </Field>
      <TextAreaField label="Description" value={value.description ?? ""} onChange={(v) => onChange({ ...value, description: v || undefined })} />
    </>
  );
}
