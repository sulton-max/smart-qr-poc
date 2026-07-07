import { ContentTypeId, contentType } from "@/domain/codes/content";
import { type ContentControlsProps, FieldRenderer } from "./fields";

// title / location are single-line; start / end are datetimes; description is a textarea (kinds come from the registry).
const fields = contentType(ContentTypeId.Calendar).fields;

/** Renders event (calendar) content — title, start/end datetimes, location, and a multiline description. */
export function CalendarControls({ fieldValues, onChange }: ContentControlsProps) {
  const setValue = (key: string, v: string) => onChange({ ...fieldValues, [key]: v });

  return (
    <>
      {fields.map((f) => (
        <FieldRenderer key={f.key} field={f} value={fieldValues[f.key]} onChange={(v) => setValue(f.key, v)} />
      ))}
    </>
  );
}
