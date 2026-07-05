import { contentType } from "@/domain/codes/content";
import { type ContentControlsProps, TextAreaField } from "./fields";

const [text] = contentType("text").fields;

/** Text content — a single free-text textarea. */
export function TextControls({ values, onChange }: ContentControlsProps) {
  return (
    <TextAreaField
      label={text.label}
      value={values[text.key] ?? ""}
      placeholder={text.placeholder}
      onChange={(v) => onChange({ ...values, [text.key]: v })}
    />
  );
}
