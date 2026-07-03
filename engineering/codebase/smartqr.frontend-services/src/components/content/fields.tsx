// Shared input primitives for the per-type content control groups. Extracted verbatim from the old
// generic `ContentTypeForm` so every control group renders identical markup (same labels, placeholders,
// required semantics, and the native-input styling) without duplicating it. The control groups still
// drive the flat `FieldValues` form-state model (a separate pass restructures the data flow later).

import type { ReactNode } from "react";
import { FormField, Select as SdkSelect, TextInput } from "@wow-two-beta/ui/forms";
import type { FieldValues } from "../../lib/contentTypes";

/** Shared props for every per-type control group — keeps the flat `FieldValues` form-state model. */
export interface ContentControlsProps {
  /** Current field values, keyed by `ContentField.key`. */
  values: FieldValues;
  /** Emit the next values record. */
  onChange: (next: FieldValues) => void;
}

// Native textarea / datetime inputs styled to match the SDK TextInput (which has no
// multiline / datetime variant yet). Plain text-likes use the SDK TextInput.
export const nativeInput =
  "w-full rounded-md border border-border bg-background px-3 py-2 text-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring";

/** A labeled row wrapping any control — thin re-export of the SDK `FormField` for a consistent import surface. */
export function Field({ label, children }: { label: string; children: ReactNode }) {
  return <FormField label={label}>{children}</FormField>;
}

/** Single-line text-like input (text / url / tel / email / number all render the SDK TextInput). */
export function TextField({
  label,
  value,
  placeholder,
  onChange,
}: {
  label: string;
  value: string;
  placeholder?: string;
  onChange: (v: string) => void;
}) {
  return (
    <Field label={label}>
      <TextInput ring="sm" value={value} placeholder={placeholder} onChange={(e) => onChange(e.target.value)} />
    </Field>
  );
}

/** Multi-line text input (native `<textarea>` styled to match the SDK TextInput). */
export function TextAreaField({
  label,
  value,
  placeholder,
  onChange,
}: {
  label: string;
  value: string;
  placeholder?: string;
  onChange: (v: string) => void;
}) {
  return (
    <Field label={label}>
      <textarea
        className={nativeInput}
        rows={3}
        value={value}
        placeholder={placeholder}
        onChange={(e) => onChange(e.target.value)}
      />
    </Field>
  );
}

/** Native `datetime-local` input styled to match the SDK TextInput. */
export function DateTimeField({
  label,
  value,
  onChange,
}: {
  label: string;
  value: string;
  onChange: (v: string) => void;
}) {
  return (
    <Field label={label}>
      <input type="datetime-local" className={nativeInput} value={value} onChange={(e) => onChange(e.target.value)} />
    </Field>
  );
}

/** SDK Select over value/label options. `value` falls back to the first option when unset (matches the old form). */
export function SelectField({
  label,
  value,
  options,
  onChange,
}: {
  label: string;
  value: string | undefined;
  options: { value: string; label: string }[];
  onChange: (v: string) => void;
}) {
  return (
    <Field label={label}>
      <SdkSelect value={value ?? options[0]?.value} onValueChange={(o) => o && onChange(o.itemKey)}>
        <SdkSelect.Trigger>
          <SdkSelect.Value />
        </SdkSelect.Trigger>
        <SdkSelect.Content>
          {options.map((o) => (
            <SdkSelect.Item key={o.value} itemKey={o.value} label={o.label} />
          ))}
        </SdkSelect.Content>
      </SdkSelect>
    </Field>
  );
}
