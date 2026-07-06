// Shared input primitives for the per-type content control groups. Extracted verbatim from the old
// generic `ContentTypeForm` so every control group renders identical markup (same labels, placeholders,
// required semantics, and the native-input styling) without duplicating it. The control groups still
// drive the flat `FieldValues` form-state model (a separate pass restructures the data flow later).

import { Field, Select as SdkSelect, TextInput } from "@wow-two-beta/ui/presentation/forms";

import type { FieldValues } from "@/domain/codes/content";

/** Shared props for every per-type control group — keeps the flat `FieldValues` form-state model. */
export interface ContentControlsProps {
  /** The current field values, keyed by `ContentField.key`. */
  readonly values: FieldValues;

  /** Emits the next values record. */
  readonly onChange: (next: FieldValues) => void;
}

// Native textarea / datetime inputs styled to match the SDK TextInput (which has no
// multiline / datetime variant yet). Plain text-likes use the SDK TextInput.
export const nativeInput =
  "w-full rounded-md border border-border bg-background px-3 py-2 text-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring";

/** Defines props for a single-line or multi-line text field. */
interface TextFieldProps {
  /** The field's label. */
  readonly label: string;

  /** The current text value. */
  readonly value: string;

  /** The placeholder shown while the field is empty. */
  readonly placeholder?: string;

  /** Emits the next text value. */
  readonly onChange: (value: string) => void;
}

/** Single-line text-like input (text / url / tel / email / number all render the SDK TextInput). */
export function TextField({ label, value, placeholder, onChange }: TextFieldProps) {
  return (
    <Field label={label}>
      <TextInput ring="sm" value={value} placeholder={placeholder} onChange={(e) => onChange(e.target.value)} />
    </Field>
  );
}

/** Multi-line text input (native `<textarea>` styled to match the SDK TextInput). */
export function TextAreaField({ label, value, placeholder, onChange }: TextFieldProps) {
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

/** Defines props for the datetime field. */
interface DateTimeFieldProps {
  /** The field's label. */
  readonly label: string;

  /** The current `datetime-local` value. */
  readonly value: string;

  /** Emits the next value. */
  readonly onChange: (value: string) => void;
}

/** Native `datetime-local` input styled to match the SDK TextInput. */
export function DateTimeField({ label, value, onChange }: DateTimeFieldProps) {
  return (
    <Field label={label}>
      <input type="datetime-local" className={nativeInput} value={value} onChange={(e) => onChange(e.target.value)} />
    </Field>
  );
}

/** Defines one selectable option. */
interface SelectOption {
  /** The option's stored value. */
  readonly value: string;

  /** The option's display label. */
  readonly label: string;
}

/** Defines props for the select field. */
interface SelectFieldProps {
  /** The field's label. */
  readonly label: string;

  /** The current value, or undefined to fall back to the first option. */
  readonly value: string | undefined;

  /** The selectable options. */
  readonly options: ReadonlyArray<SelectOption>;

  /** Emits the next value. */
  readonly onChange: (value: string) => void;
}

/** SDK Select over value/label options. `value` falls back to the first option when unset (matches the old form). */
export function SelectField({ label, value, options, onChange }: SelectFieldProps) {
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
