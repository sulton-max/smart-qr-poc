import { Field, TelInput } from "@wow-two-beta/ui/presentation/forms";
import type { PhoneContent } from "@/domain/codes/content";
import type { ContentControlsProps } from "./fields";

/** Renders phone content — a single dial number. */
export function PhoneControls({ value, onChange }: ContentControlsProps<PhoneContent>) {
  return (
    <Field label="Phone">
      <TelInput
        ring="sm"
        value={value.phone}
        placeholder="+1 555 0100"
        onChange={(e) => onChange({ ...value, phone: e.target.value })}
      />
    </Field>
  );
}
