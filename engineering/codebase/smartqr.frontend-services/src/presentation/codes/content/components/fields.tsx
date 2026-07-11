// Shared input primitives for the per-type content control groups — one small typed component per input
// shape (datetime / select) so every control renders identical markup (labels, native-input styling)
// without duplicating it. Text-likes bind the SDK atoms directly (TextInput / typed EmailInput·TelInput·
// UrlInput / TextareaInput); each `*Controls` maps these to its typed content model (`CalendarContent`, …).
//
// `DateTimeField` mirrors the SDK `DateField` shape — forwardRef + a typed `Temporal.PlainDateTime`
// value/onValueChange (not the DOM event) + `useFormControl()` wiring + `useControlled` — over a native
// `<input type="datetime-local">`. Two deviations are forced by this app's pinned deps and are drop-in
// API-compatible with the SDK: Temporal comes from `@js-temporal/polyfill` (the app's polyfill; the SDK's
// `temporal-polyfill` is not resolvable here), and styling reuses the app's `NativeInputStyles` because
// the SDK's `inputBaseVariants` is not re-exported from the pinned `@wow-two-beta/ui@0.0.95` barrel.

import { forwardRef, type InputHTMLAttributes } from "react";
import { Temporal } from "@js-temporal/polyfill";

import { Field, Select as SdkSelect } from "@wow-two-beta/ui/presentation/forms";
import { useFormControl } from "@wow-two-beta/ui/foundation/primitives";
import { useControlled } from "@wow-two-beta/ui/foundation/hooks";
import { cn } from "@wow-two-beta/ui/foundation/utils";

import { NativeInputStyles } from "./NativeInputStyles";

/** Defines the shared props for every per-type control group — its typed content model + a change handler. */
export interface ContentControlsProps<T> {
  /** The current typed content for this type. */
  readonly value: T;

  /** Emits the next typed content. */
  readonly onChange: (next: T) => void;
}

/** Formats a `PlainDateTime` to the `YYYY-MM-DDTHH:mm` string a native `datetime-local` input expects. */
export function formatISODateTime(value: Temporal.PlainDateTime | null | undefined): string {
  if (!value) return "";
  return value.toString({ smallestUnit: "minute" });
}

/** Parses a `datetime-local` `YYYY-MM-DDTHH:mm` string to a `PlainDateTime`; returns null for empty/invalid input. */
export function parseISODateTime(value: string | null | undefined): Temporal.PlainDateTime | null {
  if (!value) return null;
  try {
    return Temporal.PlainDateTime.from(value, { overflow: "reject" });
  } catch {
    return null;
  }
}

/** Defines props for the datetime field — mirrors the SDK `DateField`, adding a `label` for the `Field` wrapper. */
export interface DateTimeFieldProps
  extends Omit<InputHTMLAttributes<HTMLInputElement>, "type" | "value" | "defaultValue" | "onChange"> {
  /** The field's label. */
  readonly label: string;

  /** The controlled value. */
  value?: Temporal.PlainDateTime | null;

  /** The uncontrolled initial value. */
  defaultValue?: Temporal.PlainDateTime | null;

  /** Emits the next value — the typed `PlainDateTime`, not the DOM event. */
  onValueChange?: (value: Temporal.PlainDateTime | null) => void;
}

/**
 * Renders the native `datetime-local` control. Split out from `DateTimeField` so `useFormControl()` reads
 * the context of the enclosing `<Field>` (which provides it) rather than an outer/absent one — the correct
 * wiring that retires the previous `useNativeFieldProps` shim (which read the wrong context and left the
 * label's `htmlFor` dangling).
 */
const DateTimeInput = forwardRef<HTMLInputElement, Omit<DateTimeFieldProps, "label">>(function DateTimeInput(
  { value, defaultValue, onValueChange, className, id, disabled, required, ...rest },
  ref,
) {
  const ctx = useFormControl();
  const [current, setCurrent] = useControlled<Temporal.PlainDateTime | null>({
    controlled: value,
    default: defaultValue ?? null,
    onChange: onValueChange,
  });
  return (
    <input
      ref={ref}
      type="datetime-local"
      id={id ?? ctx?.id}
      disabled={disabled ?? ctx?.isDisabled}
      required={required ?? ctx?.isRequired}
      aria-invalid={ctx?.isInvalid || undefined}
      aria-describedby={ctx?.describedBy}
      value={formatISODateTime(current)}
      onChange={(e) => setCurrent(parseISODateTime(e.target.value))}
      className={cn(NativeInputStyles, className)}
      {...rest}
    />
  );
});

/** Renders a `datetime-local` field wired to Temporal — labelled via the SDK `Field`, mirroring `DateField`. */
export const DateTimeField = forwardRef<HTMLInputElement, DateTimeFieldProps>(function DateTimeField(
  { label, ...inputProps },
  ref,
) {
  return (
    <Field label={label}>
      <DateTimeInput ref={ref} {...inputProps} />
    </Field>
  );
});

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
