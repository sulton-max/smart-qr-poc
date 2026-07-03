import { contentType } from "../../lib/contentTypes";
import { type ContentControlsProps, SelectField, TextField } from "./fields";

// ssid / password are text; `encryption` and `hidden` are selects (options come from the registry). `hidden`
// stays a "true"/"false" string in the form state — `buildContent` maps it to a real bool on the wire.
const [ssid, password, encryption, hidden] = contentType("wifi").fields;

/** WiFi content — SSID, password, security type, and a hidden-network toggle. */
export function WifiControls({ values, onChange }: ContentControlsProps) {
  const set = (key: string, v: string) => onChange({ ...values, [key]: v });
  return (
    <>
      <TextField label={ssid.label} value={values[ssid.key] ?? ""} placeholder={ssid.placeholder} onChange={(v) => set(ssid.key, v)} />
      <TextField label={password.label} value={values[password.key] ?? ""} placeholder={password.placeholder} onChange={(v) => set(password.key, v)} />
      <SelectField label={encryption.label} value={values[encryption.key]} options={encryption.options ?? []} onChange={(v) => set(encryption.key, v)} />
      <SelectField label={hidden.label} value={values[hidden.key]} options={hidden.options ?? []} onChange={(v) => set(hidden.key, v)} />
    </>
  );
}
