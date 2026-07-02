import { FormField, Select, TextInput } from "@wow-two-beta/ui/forms";
import { contentType, type ContentTypeId, type FieldValues } from "../lib/contentTypes";
import { MobileAppFields } from "./MobileAppFields";

export interface ContentTypeFormProps {
  /** Which content type's fields to render. */
  typeId: ContentTypeId;
  /** Current field values, keyed by `ContentField.key`. */
  values: FieldValues;
  /** Emit the next values record. */
  onChange: (next: FieldValues) => void;
}

// Native textarea / datetime inputs styled to match the SDK TextInput (which has no
// multiline / datetime variant yet). Plain text-likes use the SDK TextInput.
const nativeInput =
  "w-full rounded-md border border-border bg-background px-3 py-2 text-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring";

/** Renders the form fields for a content type from its registry definition. */
export function ContentTypeForm({ typeId, values, onChange }: ContentTypeFormProps) {
  const def = contentType(typeId);
  const set = (key: string, v: string) => onChange({ ...values, [key]: v });

  return (
    <>
      {def.note && <p className="text-sm text-muted-foreground">{def.note}</p>}
      {typeId === "mobileApp" ? (
        <MobileAppFields values={values} onChange={onChange} />
      ) : (
        def.fields.map((f) => (
        <FormField key={f.key} label={f.label}>
          {f.kind === "textarea" ? (
            <textarea
              className={nativeInput}
              rows={3}
              value={values[f.key] ?? ""}
              placeholder={f.placeholder}
              onChange={(e) => set(f.key, e.target.value)}
            />
          ) : f.kind === "datetime" ? (
            <input
              type="datetime-local"
              className={nativeInput}
              value={values[f.key] ?? ""}
              onChange={(e) => set(f.key, e.target.value)}
            />
          ) : f.kind === "select" ? (
            <Select
              value={values[f.key] ?? f.options?.[0]?.value}
              onValueChange={(o) => o && set(f.key, o.itemKey)}
            >
              <Select.Trigger>
                <Select.Value />
              </Select.Trigger>
              <Select.Content>
                {f.options?.map((o) => (
                  <Select.Item key={o.value} itemKey={o.value} label={o.label} />
                ))}
              </Select.Content>
            </Select>
          ) : (
            <TextInput
              ring="sm"
              value={values[f.key] ?? ""}
              placeholder={f.placeholder}
              onChange={(e) => set(f.key, e.target.value)}
            />
          )}
        </FormField>
        ))
      )}
    </>
  );
}
