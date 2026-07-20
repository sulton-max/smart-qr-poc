import { Field, NumberInput } from "@wow-two-beta/ui/presentation/forms";
import type { GeoContent } from "@/domain/codes/content";
import type { ContentControlsProps } from "./fields";

/** Renders location content — a latitude / longitude pair. */
export function GeoControls({ value, onChange }: ContentControlsProps<GeoContent>) {
  return (
    <>
      <Field label="Latitude">
        <NumberInput
          ring="sm"
          value={value.latitude}
          min={-90}
          max={90}
          step={0.000001}
          onChange={(e) => onChange({ ...value, latitude: e.target.valueAsNumber || 0 })}
        />
      </Field>
      <Field label="Longitude">
        <NumberInput
          ring="sm"
          value={value.longitude}
          min={-180}
          max={180}
          step={0.000001}
          onChange={(e) => onChange({ ...value, longitude: e.target.valueAsNumber || 0 })}
        />
      </Field>
    </>
  );
}
