import { Field, TextInput } from "@wow-two-beta/ui/presentation/forms";
import type { GeoContent } from "@/domain/codes/content";
import type { ContentControlsProps } from "./fields";

/** Renders location content — a latitude / longitude pair. */
export function GeoControls({ value, onChange }: ContentControlsProps<GeoContent>) {
  return (
    <>
      <Field label="Latitude">
        <TextInput ring="sm" value={value.latitude} onChange={(e) => onChange({ ...value, latitude: e.target.value })} />
      </Field>
      <Field label="Longitude">
        <TextInput ring="sm" value={value.longitude} onChange={(e) => onChange({ ...value, longitude: e.target.value })} />
      </Field>
    </>
  );
}
