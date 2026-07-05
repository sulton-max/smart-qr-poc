import { contentType } from "@/domain/codes/content";
import { type ContentControlsProps, TextField } from "./fields";

const [latitude, longitude] = contentType("geo").fields;

/** Location content — latitude / longitude coordinate pair. */
export function GeoControls({ values, onChange }: ContentControlsProps) {
  const set = (key: string, v: string) => onChange({ ...values, [key]: v });
  return (
    <>
      <TextField label={latitude.label} value={values[latitude.key] ?? ""} placeholder={latitude.placeholder} onChange={(v) => set(latitude.key, v)} />
      <TextField label={longitude.label} value={values[longitude.key] ?? ""} placeholder={longitude.placeholder} onChange={(v) => set(longitude.key, v)} />
    </>
  );
}
