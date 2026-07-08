// Shared input primitives for the per-type content control groups. Extracted verbatim from the old
// generic `ContentControls` so every control group renders identical markup (same labels, placeholders,
// required semantics, and the native-input styling) without duplicating it. The control groups still
// drive the flat `FieldValues` form-state model (a separate pass restructures the data flow later).

import { Field, Select as SdkSelect, TextInput } from "@wow-two-beta/ui/presentation/forms";
import { useFormControl } from "@wow-two-beta/ui/foundation/primitives";

import { FieldKind, type ContentField, type FieldValues } from "@/domain/codes/content";
import { NativeInputStyles } from "./NativeInputStyles";

/**
 * Provides access to the enclosing SDK `Field` context as spreadable native-element props (id + aria).
 * SDK atoms (TextInput, Select) auto-wire via context; a raw `<textarea>` / `<input>` does not, so without
 * this the `<label>`'s `htmlFor` points at an element with no matching `id` and the label goes unassociated.
 */
function useNativeFieldProps() {
  const control = useFormControl();
  if (!control) return {};
  return {
    id: control.id,
    "aria-invalid": control.isInvalid || undefined,
    "aria-describedby": control.isInvalid ? control.errorId : undefined,
    required: control.isRequired || undefined,
    disabled: control.isDisabled || undefined,
    readOnly: control.isReadOnly || undefined,
  };
}

/** Defines the shared props for every per-type control group — keeps the flat `FieldValues` form-state model. */
export interface ContentControlsProps {
  /** The current field values, keyed by `ContentField.key`. */
  readonly fieldValues: FieldValues;

  /** Emits the next values record. */
  readonly onChange: (next: FieldValues) => void;
}

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

/** Renders a single-line text-like input (text / url / tel / email / number all render the SDK TextInput). */
export function TextField({ label, value, placeholder, onChange }: TextFieldProps) {
  return (
    <Field label={label}>
      <TextInput ring="sm" value={value} placeholder={placeholder} onChange={(e) => onChange(e.target.value)} />
    </Field>
  );
}

/** Renders a multi-line text input (native `<textarea>` styled to match the SDK TextInput). */
export function TextAreaField({ label, value, placeholder, onChange }: TextFieldProps) {
  const fieldProps = useNativeFieldProps();
  return (
    <Field label={label}>
      <textarea
        {...fieldProps}
        className={NativeInputStyles}
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

/** Renders a native `datetime-local` input styled to match the SDK TextInput. */
export function DateTimeField({ label, value, onChange }: DateTimeFieldProps) {
  const fieldProps = useNativeFieldProps();
  return (
    <Field label={label}>
      <input
        type="datetime-local"
        {...fieldProps}
        className={NativeInputStyles}
        value={value}
        onChange={(e) => onChange(e.target.value)}
      />
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

/** Renders an SDK Select over value/label options. `value` falls back to the first option when unset (matches the old form). */
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

/** Defines props for the field renderer — one registry `ContentField` bound to its current value. */
interface FieldRendererProps {
  /** The field definition (label, kind, placeholder, options) driving which input renders. */
  readonly field: ContentField;

  /** The field's current string value. */
  readonly value: string | undefined;

  /** Emits the field's next string value. */
  readonly onChange: (value: string) => void;
}

/** Renders the right input for a registry field, dispatching on its `FieldKind` (textarea / datetime / select / text-like). */
export function FieldRenderer({ field, value, onChange }: FieldRendererProps) {
  switch (field.kind) {
    case FieldKind.TextArea:
      return <TextAreaField label={field.label} value={value ?? ""} placeholder={field.placeholder} onChange={onChange} />;
    case FieldKind.DateTime:
      return <DateTimeField label={field.label} value={value ?? ""} onChange={onChange} />;
    case FieldKind.Select:
      return <SelectField label={field.label} value={value} options={field.options ?? []} onChange={onChange} />;
    default:
      return <TextField label={field.label} value={value ?? ""} placeholder={field.placeholder} onChange={onChange} />;
  }
}
