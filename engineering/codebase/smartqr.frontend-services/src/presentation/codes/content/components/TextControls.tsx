import { Field, TextareaInput } from "@wow-two-beta/ui/presentation/forms";
import type { TextContent } from "@/domain/codes/content";
import type { ContentControlsProps } from "./fields";

/** Renders text content — a single free-text payload. */
export function TextControls({ value, onChange }: ContentControlsProps<TextContent>) {
  return (
    <Field label="Text">
      <TextareaInput ring="sm" rows={3} value={value.text} onChange={(e) => onChange({ ...value, text: e.target.value })} />
    </Field>
  );
}
