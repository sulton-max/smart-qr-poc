import { contentType } from "../../lib/contentTypes";
import { type ContentControlsProps, DateTimeField, TextAreaField, TextField } from "./fields";

const [title, start, end, location, description] = contentType("calendar").fields;

/** Event (calendar) content — title, start/end datetimes, location, and a multiline description. */
export function CalendarControls({ values, onChange }: ContentControlsProps) {
  const set = (key: string, v: string) => onChange({ ...values, [key]: v });
  return (
    <>
      <TextField label={title.label} value={values[title.key] ?? ""} placeholder={title.placeholder} onChange={(v) => set(title.key, v)} />
      <DateTimeField label={start.label} value={values[start.key] ?? ""} onChange={(v) => set(start.key, v)} />
      <DateTimeField label={end.label} value={values[end.key] ?? ""} onChange={(v) => set(end.key, v)} />
      <TextField label={location.label} value={values[location.key] ?? ""} placeholder={location.placeholder} onChange={(v) => set(location.key, v)} />
      <TextAreaField label={description.label} value={values[description.key] ?? ""} placeholder={description.placeholder} onChange={(v) => set(description.key, v)} />
    </>
  );
}
