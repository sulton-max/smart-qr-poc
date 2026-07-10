import { Field, TextInput } from "@wow-two-beta/ui/presentation/forms";
import type { WifiContent } from "@/domain/codes/content";
import { type ContentControlsProps, type SelectOption, SelectField } from "./fields";

const ENCRYPTION_OPTIONS: readonly SelectOption[] = [
  { value: "WPA", label: "WPA/WPA2" },
  { value: "WEP", label: "WEP" },
  { value: "nopass", label: "None" },
];

const HIDDEN_OPTIONS: readonly SelectOption[] = [
  { value: "false", label: "No" },
  { value: "true", label: "Yes" },
];

/** Renders WiFi content — SSID, optional password, security type, and a hidden-network toggle (`hidden` is a real bool). */
export function WifiControls({ value, onChange }: ContentControlsProps<WifiContent>) {
  return (
    <>
      <Field label="Network name (SSID)">
        <TextInput ring="sm" value={value.ssid} onChange={(e) => onChange({ ...value, ssid: e.target.value })} />
      </Field>
      <Field label="Password">
        <TextInput ring="sm" value={value.password ?? ""} onChange={(e) => onChange({ ...value, password: e.target.value || undefined })} />
      </Field>
      <SelectField
        label="Security"
        value={value.encryption}
        options={ENCRYPTION_OPTIONS}
        onChange={(encryption) => onChange({ ...value, encryption })}
      />
      <SelectField
        label="Hidden network"
        value={String(value.hidden)}
        options={HIDDEN_OPTIONS}
        onChange={(v) => onChange({ ...value, hidden: v === "true" })}
      />
    </>
  );
}
