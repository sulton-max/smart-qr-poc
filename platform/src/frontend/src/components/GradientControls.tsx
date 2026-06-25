import { SegmentedControl, ToggleButton } from "@wow-two-beta/ui/actions";
import { ColorPicker, FormField } from "@wow-two-beta/ui/forms";
import { Text } from "@wow-two-beta/ui/display";
import { Grid, Stack } from "@wow-two-beta/ui/layout";
import { GradientType, type PreviewGradient } from "../types";

export interface GradientControlsProps {
  /** The current foreground gradient, or `null` for a solid foreground. */
  gradient: PreviewGradient | null;
  /** The solid foreground — seeds the gradient's first stop when first enabled. */
  foreground: string;
  /** Emit the next gradient, or `null` to fall back to the solid foreground. */
  onChange: (gradient: PreviewGradient | null) => void;
}

// Linear-gradient direction presets (degrees) with an arrow glyph hint.
const ANGLES: { value: number; label: string }[] = [
  { value: 0, label: "→" },
  { value: 45, label: "↘" },
  { value: 90, label: "↓" },
  { value: 135, label: "↙" },
];

const FILL_SOLID = "solid";
const FILL_GRADIENT = "gradient";

/**
 * Foreground-fill controls (v0.5) — switch between a solid color and a two-stop
 * linear/radial gradient. Drives the live preview `style.gradient`; `null` = solid.
 */
export function GradientControls({ gradient, foreground, onChange }: GradientControlsProps) {
  const isGradient = gradient !== null;

  function enable() {
    onChange({
      type: GradientType.Linear,
      angle: 45,
      stops: [
        { color: foreground, offset: 0 },
        { color: "#7c3aed", offset: 1 },
      ],
    });
  }

  function patch(next: Partial<PreviewGradient>) {
    if (gradient) onChange({ ...gradient, ...next });
  }

  function setStop(index: 0 | 1, color: string) {
    if (!gradient) return;
    const stops = gradient.stops.slice();
    stops[index] = { ...stops[index], color };
    onChange({ ...gradient, stops });
  }

  return (
    <Stack gap="4">
      <FormField label="Fill" helper="A solid foreground, or a two-color gradient.">
        <SegmentedControl
          type="single"
          value={isGradient ? FILL_GRADIENT : FILL_SOLID}
          onValueChange={(v) => {
            if (v === FILL_GRADIENT) enable();
            else if (v === FILL_SOLID) onChange(null);
          }}
          aria-label="Foreground fill"
        >
          <ToggleButton value={FILL_SOLID} className="flex-1">Solid</ToggleButton>
          <ToggleButton value={FILL_GRADIENT} className="flex-1">Gradient</ToggleButton>
        </SegmentedControl>
      </FormField>

      {gradient && (
        <>
          <SegmentedControl
            type="single"
            value={gradient.type}
            onValueChange={(v) => v && patch({ type: v as PreviewGradient["type"] })}
            aria-label="Gradient type"
          >
            <ToggleButton value={GradientType.Linear} className="flex-1">Linear</ToggleButton>
            <ToggleButton value={GradientType.Radial} className="flex-1">Radial</ToggleButton>
          </SegmentedControl>

          <Grid columns="2" gap="4">
            <FormField label="From">
              <ColorPicker
                value={gradient.stops[0]?.color ?? foreground}
                onValueChange={(hex) => setStop(0, hex ?? "#000000")}
              />
            </FormField>
            <FormField label="To">
              <ColorPicker
                value={gradient.stops[1]?.color ?? "#7c3aed"}
                onValueChange={(hex) => setStop(1, hex ?? "#7c3aed")}
              />
            </FormField>
          </Grid>

          {gradient.type === GradientType.Linear && (
            <FormField label="Direction">
              <SegmentedControl
                type="single"
                value={String(gradient.angle)}
                onValueChange={(v) => v && patch({ angle: Number(v) })}
                aria-label="Gradient direction"
              >
                {ANGLES.map((a) => (
                  <ToggleButton
                    key={a.value}
                    value={String(a.value)}
                    title={`${a.value}°`}
                    className="flex-1"
                  >
                    <Text as="span" size="sm">
                      {a.label}
                    </Text>
                  </ToggleButton>
                ))}
              </SegmentedControl>
            </FormField>
          )}
        </>
      )}
    </Stack>
  );
}
