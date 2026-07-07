import { Field, Select, TextInput } from "@wow-two-beta/ui/presentation/forms";
import type { FieldValues } from "@/domain/codes/content";
import type { ContentControlsProps } from "./fields";

/** Defines one mobile-app destination — a store link plus the "default for other devices" option label. */
interface MobileAppInput {
  /** The values-record key the input binds to. */
  readonly key: string;

  /** The input's field label. */
  readonly label: string;

  /** The label shown for this link in the fallback picker. */
  readonly fallbackLabel: string;

  /** The input's placeholder. */
  readonly placeholder: string;
}

// The three destination inputs. The fallback is chosen among the links the user actually filled; "other" is
// an optional custom catch-all. Order defines the default (first filled link wins).
const MobileAppInputs: readonly MobileAppInput[] = [
  { key: "appStore", label: "App Store (iOS) URL", fallbackLabel: "App Store (iOS)", placeholder: "https://apps.apple.com/app/…" },
  { key: "playStore", label: "Google Play URL", fallbackLabel: "Google Play", placeholder: "https://play.google.com/store/apps/…" },
  { key: "other", label: "Other devices URL (optional)", fallbackLabel: "Other devices URL", placeholder: "https://yourapp.com or another store" },
];

/** Resolves the active fallback key: the saved choice if it's still a filled link, else the first filled link. */
function getActiveFallback(options: readonly MobileAppInput[], values: FieldValues): string | undefined {
  const filled = options.filter((o) => (values[o.key] ?? "").trim());
  return filled.some((o) => o.key === values.fallback) ? values.fallback : filled[0]?.key;
}

/**
 * Renders mobile-app-link controls — store links plus a "default for other devices" picker chosen among the
 * filled links (the separate "other" URL is optional). The backend derives the device rules + fallback.
 */
export function MobileAppControls({ fieldValues, onChange }: ContentControlsProps) {
  const filled = MobileAppInputs.filter((o) => (fieldValues[o.key] ?? "").trim());
  const activeFallback = getActiveFallback(MobileAppInputs, fieldValues);

  // Merge a link change, keeping `fallback` pointing at a link that's still filled.
  const setLink = (key: string, v: string) => {
    const next: FieldValues = { ...fieldValues, [key]: v };
    next.fallback = getActiveFallback(MobileAppInputs, next);
    onChange(next);
  };

  return (
    <>
      {MobileAppInputs.map((input) => (
        <Field key={input.key} label={input.label}>
          <TextInput
            ring="sm"
            value={fieldValues[input.key] ?? ""}
            placeholder={input.placeholder}
            onChange={(e) => setLink(input.key, e.target.value)}
          />
        </Field>
      ))}

      {/* Only a real choice (≥2 filled links) needs a picker; one link is the fallback by default. */}
      {filled.length > 1 && (
        <Field label="Other devices open">
          <Select value={activeFallback} onValueChange={(o) => o && onChange({ ...fieldValues, fallback: o.itemKey })}>
            <Select.Trigger>
              <Select.Value />
            </Select.Trigger>
            <Select.Content>
              {filled.map((o) => (
                <Select.Item key={o.key} itemKey={o.key} label={o.fallbackLabel} />
              ))}
            </Select.Content>
          </Select>
        </Field>
      )}
    </>
  );
}
