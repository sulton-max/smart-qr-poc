import { ContentTypeId, contentType } from "@/domain/codes/content";
import { type ContentControlsProps, FieldRenderer } from "./fields";

// ssid / password are text; `encryption` and `hidden` are selects (options come from the registry). `hidden`
// stays a "true"/"false" string in the form state — `buildContent` maps it to a real bool on the wire.
const fields = contentType(ContentTypeId.Wifi).fields;

/** Renders WiFi content — SSID, password, security type, and a hidden-network toggle. */
export function WifiControls({ fieldValues, onChange }: ContentControlsProps) {
  const setValue = (key: string, v: string) => onChange({ ...fieldValues, [key]: v });

  return (
    <>
      {fields.map((f) => (
        <FieldRenderer key={f.key} field={f} value={fieldValues[f.key]} onChange={(v) => setValue(f.key, v)} />
      ))}
    </>
  );
}
