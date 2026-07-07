import { ContentTypeId, contentType } from "@/domain/codes/content";
import { type ContentControlsProps, FieldRenderer } from "./fields";

// latitude / longitude are numeric coordinate fields (kinds come from the registry).
const fields = contentType(ContentTypeId.Geo).fields;

/** Renders location content — latitude / longitude coordinate pair. */
export function GeoControls({ fieldValues, onChange }: ContentControlsProps) {
  const setValue = (key: string, v: string) => onChange({ ...fieldValues, [key]: v });

  return (
    <>
      {fields.map((f) => (
        <FieldRenderer key={f.key} field={f} value={fieldValues[f.key]} onChange={(v) => setValue(f.key, v)} />
      ))}
    </>
  );
}
