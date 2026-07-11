import { Field, TextInput, TextareaInput } from "@wow-two-beta/ui/presentation/forms";
import type { CalendarContent } from "@/domain/codes/content";
import { type ContentControlsProps, DateTimeField, formatISODateTime, parseISODateTime } from "./fields";

/** Renders calendar-event content — title + start required, everything else optional. */
export function CalendarControls({ value, onChange }: ContentControlsProps<CalendarContent>) {
  return (
    <>
      <Field label="Title">
        <TextInput ring="sm" value={value.title} onChange={(e) => onChange({ ...value, title: e.target.value })} />
      </Field>
      <DateTimeField
        label="Starts"
        value={value.start ? parseISODateTime(value.start) : null}
        onValueChange={(dt) => onChange({ ...value, start: dt ? formatISODateTime(dt) : "" })}
      />
      <DateTimeField
        label="Ends"
        value={value.end ? parseISODateTime(value.end) : null}
        onValueChange={(dt) => onChange({ ...value, end: dt ? formatISODateTime(dt) : undefined })}
      />
      <Field label="Location">
        <TextInput ring="sm" value={value.location ?? ""} onChange={(e) => onChange({ ...value, location: e.target.value || undefined })} />
      </Field>
      <Field label="Description">
        <TextareaInput ring="sm" rows={3} value={value.description ?? ""} onChange={(e) => onChange({ ...value, description: e.target.value || undefined })} />
      </Field>
    </>
  );
}
