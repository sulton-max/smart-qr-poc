import { DateTimeField, Field, TextInput, TextAreaInput } from "@wow-two-beta/ui/presentation/forms";
import type { CalendarContent } from "@/domain/codes/content";
import type { ContentControlsProps } from "./fields";

/** Renders calendar-event content — title + start required, everything else optional. */
export function CalendarControls({ value, onChange }: ContentControlsProps<CalendarContent>) {
  return (
    <>
      <Field label="Title">
        <TextInput ring="sm" value={value.title} onChange={(e) => onChange({ ...value, title: e.target.value })} />
      </Field>
      <Field label="Starts">
        <DateTimeField
          value={value.start}
          onValueChange={(dt) => dt && onChange({ ...value, start: dt })}
        />
      </Field>
      <Field label="Ends">
        <DateTimeField
          value={value.end ?? null}
          onValueChange={(dt) => onChange({ ...value, end: dt ?? undefined })}
        />
      </Field>
      <Field label="Location">
        <TextInput ring="sm" value={value.location ?? ""} onChange={(e) => onChange({ ...value, location: e.target.value || undefined })} />
      </Field>
      <Field label="Description">
        <TextAreaInput ring="sm" rows={3} value={value.description ?? ""} onChange={(e) => onChange({ ...value, description: e.target.value || undefined })} />
      </Field>
    </>
  );
}
