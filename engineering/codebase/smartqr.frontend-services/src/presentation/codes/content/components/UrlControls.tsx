import { ContentTypeId, contentType } from "@/domain/codes/content";
import { type ContentControlsProps, FieldRenderer } from "./fields";

const [url] = contentType(ContentTypeId.Url).fields;

/** Renders URL content — the dynamic forwarder's single destination field. */
export function UrlControls({ fieldValues, onChange }: ContentControlsProps) {
  return (
    <FieldRenderer field={url} value={fieldValues[url.key]} onChange={(v) => onChange({ ...fieldValues, [url.key]: v })} />
  );
}
