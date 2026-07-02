import { FormField, Select, TextInput } from "@wow-two-beta/ui/forms";
import type { FieldValues } from "../lib/contentTypes";

// The fallback destination is chosen among the links the user actually filled; "other" is an
// optional custom catch-all. Order defines the default (first filled link wins).
const FALLBACK_OPTIONS = [
  { key: "ios", label: "App Store (iOS)" },
  { key: "android", label: "Google Play" },
  { key: "other", label: "Other devices URL" },
] as const;

export interface MobileAppFieldsProps {
  /** Current field values (`ios` / `android` / `other` URLs + the `fallback` key). */
  values: FieldValues;
  /** Emit the next values record. */
  onChange: (next: FieldValues) => void;
}

/**
 * Mobile-app-link fields — store links plus a "default for other devices" picker chosen among the
 * filled links (the separate "other" URL is optional). The backend derives the device rules + fallback.
 */
export function MobileAppFields({ values, onChange }: MobileAppFieldsProps) {
  const filled = FALLBACK_OPTIONS.filter((o) => (values[o.key] ?? "").trim());
  // The active fallback: the saved choice if it's still a filled link, else the first filled link.
  const fallback = filled.some((o) => o.key === values.fallback) ? values.fallback : filled[0]?.key;

  // Merge a link change, keeping `fallback` pointing at a link that's still filled.
  const setLink = (key: string, v: string) => {
    const next: FieldValues = { ...values, [key]: v };
    const stillFilled = FALLBACK_OPTIONS.filter((o) => (next[o.key] ?? "").trim());
    if (!stillFilled.some((o) => o.key === next.fallback)) next.fallback = stillFilled[0]?.key;
    onChange(next);
  };

  return (
    <>
      <FormField label="App Store (iOS) URL">
        <TextInput
          ring="sm"
          value={values.ios ?? ""}
          placeholder="https://apps.apple.com/app/…"
          onChange={(e) => setLink("ios", e.target.value)}
        />
      </FormField>
      <FormField label="Google Play URL">
        <TextInput
          ring="sm"
          value={values.android ?? ""}
          placeholder="https://play.google.com/store/apps/…"
          onChange={(e) => setLink("android", e.target.value)}
        />
      </FormField>
      <FormField label="Other devices URL (optional)">
        <TextInput
          ring="sm"
          value={values.other ?? ""}
          placeholder="https://yourapp.com or another store"
          onChange={(e) => setLink("other", e.target.value)}
        />
      </FormField>

      {/* Only a real choice (≥2 filled links) needs a picker; one link is the fallback by default. */}
      {filled.length > 1 && (
        <FormField label="Other devices open">
          <Select value={fallback} onValueChange={(o) => o && onChange({ ...values, fallback: o.itemKey })}>
            <Select.Trigger>
              <Select.Value />
            </Select.Trigger>
            <Select.Content>
              {filled.map((o) => (
                <Select.Item key={o.key} itemKey={o.key} label={o.label} />
              ))}
            </Select.Content>
          </Select>
        </FormField>
      )}
    </>
  );
}
