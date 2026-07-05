import { contentType, type FieldValues } from "@/domain/codes/content";
import { type ContentControlsProps, TextField } from "./fields";

const [url] = contentType("url").fields;

/** URL content — the dynamic forwarder's single destination field. */
export function UrlControls({ values, onChange }: ContentControlsProps) {
  const set = (key: string, v: string) => onChange({ ...values, [key]: v } as FieldValues);
  return (
    <TextField
      label={url.label}
      value={values[url.key] ?? ""}
      placeholder={url.placeholder}
      onChange={(v) => set(url.key, v)}
    />
  );
}
