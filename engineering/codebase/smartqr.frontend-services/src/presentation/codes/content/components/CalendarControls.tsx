import { Temporal } from "temporal-polyfill";
import { DateTimeField, Field, TextInput, TextAreaInput } from "@wow-two-beta/ui/presentation/forms";
import type { CalendarContent } from "@/domain/codes/content";
import type { ContentControlsProps } from "./fields";

/** Parses a `datetime-local` string to a `PlainDateTime`, or null for empty/invalid input. */
function parseDateTime(value: string | undefined): Temporal.PlainDateTime | null {
  if (!value) return null;
  try {
    return Temporal.PlainDateTime.from(value);
  } catch {
    return null;
  }
}

/** Formats a `PlainDateTime` to the `YYYY-MM-DDTHH:mm` string the content model stores. */
function formatDateTime(value: Temporal.PlainDateTime | null): string {
  return value ? value.toString({ smallestUnit: "minute" }) : "";
}

/** Renders calendar-event content — title + start required, everything else optional. */
export function CalendarControls({ value, onChange }: ContentControlsProps<CalendarContent>) {
  return (
    <>
      <Field label="Title">
        <TextInput ring="sm" value={value.title} onChange={(e) => onChange({ ...value, title: e.target.value })} />
      </Field>
      <Field label="Starts">
        <DateTimeField
          value={parseDateTime(value.start)}
          onValueChange={(dt) => onChange({ ...value, start: formatDateTime(dt) })}
        />
      </Field>
      <Field label="Ends">
        <DateTimeField
          value={parseDateTime(value.end)}
          onValueChange={(dt) => onChange({ ...value, end: formatDateTime(dt) || undefined })}
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
