// Shared input primitives for the per-type content control groups — one small typed component per input
// shape (text / textarea / datetime / select) so every control renders identical markup (labels,
// placeholders, native-input styling) without duplicating it. Each `*Controls` binds these to its typed
// content model (`UrlContent`, `WifiContent`, …).

import { Field, Select as SdkSelect } from "@wow-two-beta/ui/presentation/forms";
import { useFormControl } from "@wow-two-beta/ui/foundation/primitives";

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

/** Defines the shared props for every per-type control group — its typed content model + a change handler. */
export interface ContentControlsProps<T> {
  /** The current typed content for this type. */
  readonly value: T;

  /** Emits the next typed content. */
  readonly onChange: (next: T) => void;
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
export interface SelectOption {
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
      <SdkSelect
        value={value ?? options[0]?.value}
        onValueChange={(o) => o && onChange(o.itemKey)}
        getOptionLabel={(v) => options.find((o) => o.value === v)?.label}
      >
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
