// Shared input primitives for the per-type content control groups. Text-likes bind the SDK atoms
// directly (TextInput / typed EmailInput·TelInput·UrlInput / TextAreaInput) in each `*Controls`, and
// datetime is the SDK `DateTimeField` atom (Calendar bridges the string content model to it locally).
// What's left here is only what the SDK atoms don't cover: the shared `ContentControlsProps` shape and
// `SelectField` — a labelled SDK `Select` over value/label options; it hides real compound structure
// (Field + Trigger + Content + first-option fallback), so it earns its wrapper.

import { Field, Select as SdkSelect } from "@wow-two-beta/ui/presentation/forms";

/** Defines the shared props for every per-type control group — its typed content model + a change handler. */
export interface ContentControlsProps<T> {
  /** The current typed content for this type. */
  readonly value: T;

  /** Emits the next typed content. */
  readonly onChange: (next: T) => void;
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
        options={options.map((o) => ({ itemKey: o.value, value: o.value, label: o.label }))}
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
